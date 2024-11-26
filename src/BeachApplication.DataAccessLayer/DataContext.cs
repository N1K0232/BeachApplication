using System.Data;
using System.Reflection;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer.Entities.Common;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace BeachApplication.DataAccessLayer;

public class DataContext(DbContextOptions<DataContext> options, IUserService userService) : DbContext(options), IDataContext
{
    private static readonly MethodInfo setQueryFilterOnTenantEntity = typeof(DataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnTenantEntity));

    private readonly Guid tenantId = userService.GetTenantId();

    private CancellationTokenSource tokenSource = new CancellationTokenSource();
    private IDbContextTransaction transaction;

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

            if (entry.State is EntityState.Added)
            {
                if (entity is TenantEntity tenantEntity)
                {
                    tenantEntity.TenantId = tenantId;
                }
            }

            if (entry.State is EntityState.Modified)
            {
                entity.LastModifiedAt = DateTime.UtcNow;
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
        optionsBuilder.EnableDetailedErrors();

        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var type in entities.Select(t => t.ClrType))
        {
            var methods = SetGlobalQueryFiltersMethod(type);
            foreach (var method in methods)
            {
                method.MakeGenericMethod(type).Invoke(this, [modelBuilder]);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static IEnumerable<MethodInfo> SetGlobalQueryFiltersMethod(Type type)
    {
        var methods = new List<MethodInfo>();

        if (typeof(TenantEntity).IsAssignableFrom(type))
        {
            methods.Add(setQueryFilterOnTenantEntity);
        }

        return methods;
    }

    private void SetQueryFilterOnTenantEntity<T>(ModelBuilder builder) where T : TenantEntity
    {
        builder.Entity<T>().HasQueryFilter(x => x.TenantId == tenantId);
    }
}