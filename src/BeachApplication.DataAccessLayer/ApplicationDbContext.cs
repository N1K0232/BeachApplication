using System.Reflection;
using BeachApplication.DataAccessLayer.Entities.Common;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace BeachApplication.DataAccessLayer;

public class ApplicationDbContext : AuthenticationDbContext, IApplicationDbContext, IDataProtectionKeyContext
{
    private static readonly MethodInfo setQueryFilterOnDeletableEntity = typeof(ApplicationDbContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private CancellationTokenSource tokenSource = new CancellationTokenSource();
    private IDbContextTransaction transaction;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public Task DeleteAsync<T>(T entity) where T : BaseEntity
    {
        Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async ValueTask<T> GetAsync<T>(Guid id) where T : BaseEntity
    {
        var entity = await Set<T>().FindAsync([id], tokenSource.Token);
        return entity;
    }

    public IQueryable<T> GetData<T>(bool ignoreQueryFilters = false, bool trackingChanges = false) where T : BaseEntity
    {
        var set = Set<T>().AsQueryable();

        if (ignoreQueryFilters)
        {
            set = set.IgnoreQueryFilters();
        }

        return trackingChanges ? set.AsTracking() : set.AsNoTrackingWithIdentityResolution();
    }

    public async Task InsertAsync<T>(T entity) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await Set<T>().AddAsync(entity, tokenSource.Token);
    }

    public async Task SaveAsync()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.Entity.GetType()))
            .ToList();

        foreach (var entry in entries.Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var entity = entry.Entity as BaseEntity;

            if (entry.State is EntityState.Modified)
            {
                if (entity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = false;
                    deletableEntity.DeletedAt = null;
                }

                entity.LastModifiedAt = DateTime.UtcNow;
            }

            if (entry.State is EntityState.Deleted)
            {
                if (entity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = true;
                    deletableEntity.DeletedAt = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                }
            }
        }

        await SaveChangesAsync(true, tokenSource.Token);
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            transaction = await Database.BeginTransactionAsync(tokenSource.Token);
            await action.Invoke();
            await transaction.CommitAsync(tokenSource.Token);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        if (tokenSource is not null)
        {
            tokenSource.Dispose();
            tokenSource = null;
        }

        if (transaction is not null)
        {
            await transaction.DisposeAsync();
            transaction = null;
        }

        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DataProtectionKey>(b =>
        {
            b.ToTable("DataProtectionKeys");
            b.HasKey(k => k.Id);
            b.Property(k => k.Id).UseIdentityColumn(1, 1);

            b.Property(k => k.FriendlyName).HasMaxLength(256).IsRequired(false);
            b.Property(k => k.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        });

        OnModelCreatingInternal(builder);
        base.OnModelCreating(builder);
    }

    private static IEnumerable<MethodInfo> SetGlobalQueryFiltersMethod(Type type)
    {
        var methods = new List<MethodInfo>();

        if (typeof(DeletableEntity).IsAssignableFrom(type))
        {
            methods.Add(setQueryFilterOnDeletableEntity);
        }

        return methods;
    }

    private void SetQueryFilterOnDeletableEntity<T>(ModelBuilder builder) where T : DeletableEntity
    {
        builder.Entity<T>().HasQueryFilter(x => !x.IsDeleted && x.DeletedAt == null);
    }

    private void OnModelCreatingInternal(ModelBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        builder.ApplyConfigurationsFromAssembly(assembly);

        var entities = builder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)).ToList();

        foreach (var type in entities.Select(t => t.ClrType))
        {
            var methods = SetGlobalQueryFiltersMethod(type);
            foreach (var method in methods)
            {
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, [builder]);
            }
        }
    }

    private IEnumerable<EntityEntry> GetEntries(Type entityType)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => entityType.IsAssignableFrom(e.Entity.GetType()))
            .ToList();

        return entries.Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }
}