using System.Reflection;
using BeachApplication.Authentication;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.DataAccessLayer.Entities.Common;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace BeachApplication.DataAccessLayer;

public class ApplicationDbContext : AuthenticationDbContext, IApplicationDbContext
{
    private static readonly MethodInfo setQueryFilterOnDeletableEntity = typeof(ApplicationDbContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private readonly ISqlClientCache cache;
    private readonly ILogger<ApplicationDbContext> logger;
    private CancellationTokenSource tokenSource = new CancellationTokenSource();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        ISqlClientCache cache,
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        this.cache = cache;
        this.logger = logger;
    }

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

    public async Task<T> GetAsync<T>(Guid id) where T : BaseEntity
    {
        var entity = await Set<T>().FindAsync([id], tokenSource.Token);
        return entity;
    }

    public IQueryable<T> GetData<T>(bool ignoreQueryFilters = false, bool trackingChanges = false, string sql = null, params object[] parameters) where T : BaseEntity
    {
        var set = GenerateQuery<T>(sql, parameters);

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
        var entries = GetEntries(typeof(BaseEntity));

        foreach (var entry in entries)
        {
            var entity = (entry.Entity as BaseEntity)!;

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
                    entry.State = EntityState.Modified;
                    deletableEntity.IsDeleted = true;
                    deletableEntity.DeletedAt = DateTime.UtcNow;
                }
            }
        }

        var affectedRows = await SaveChangesAsync(true, tokenSource.Token);
        logger.LogInformation("saved {affectedRows} in the database", affectedRows);
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync(tokenSource.Token);
            await action.Invoke();
            await transaction.CommitAsync(tokenSource.Token);
        });
    }

    public override void Dispose()
    {
        tokenSource.Dispose();
        tokenSource = null!;

        base.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
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

    private IQueryable<T> GenerateQuery<T>(string sql, params object[] parameters) where T : BaseEntity
    {
        var set = Set<T>();

        if (!string.IsNullOrWhiteSpace(sql) && parameters.Length > 0)
        {
            return set.FromSqlRaw(sql, parameters);
        }

        return set;
    }

    private IEnumerable<EntityEntry> GetEntries(Type entityType)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => entityType.IsAssignableFrom(e.Entity.GetType()))
            .ToList();

        return entries.Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }
}