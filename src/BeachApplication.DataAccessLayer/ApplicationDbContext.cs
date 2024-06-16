using System.Reflection;
using BeachApplication.Authentication;
using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.DataAccessLayer;

public class ApplicationDbContext : AuthenticationDbContext, IApplicationDbContext
{
    private static readonly MethodInfo setQueryFilterOnDeletableEntity = typeof(ApplicationDbContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private CancellationTokenSource tokenSource = new CancellationTokenSource();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public Task DeleteAsync<T>(T entity) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Set<T>().Remove(entity);

        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        Set<T>().RemoveRange(entities);

        return Task.CompletedTask;
    }

    public async ValueTask<T> GetAsync<T>(Guid id) where T : BaseEntity
    {
        var entity = await Set<T>().FindAsync([id], tokenSource.Token);
        return entity;
    }

    public IQueryable<T> GetData<T>(bool ignoreQueryFilters = false, bool trackingChanges = false, string sql = null, params object[] parameters) where T : BaseEntity
    {
        var set = !string.IsNullOrWhiteSpace(sql) && parameters.Length > 0 ? Set<T>().FromSqlRaw(sql, parameters) : Set<T>();

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

    public async Task<int> SaveAsync()
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
                    deletableEntity.DeletedDate = null;
                }

                entity.LastModificationDate = DateTime.UtcNow;
            }

            if (entry.State is EntityState.Deleted)
            {
                if (entity is DeletableEntity deletableEntity)
                {
                    entry.State = EntityState.Modified;
                    deletableEntity.IsDeleted = true;
                    deletableEntity.DeletedDate = DateTime.UtcNow;
                }
            }
        }

        return await SaveChangesAsync(true, tokenSource.Token);
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
        tokenSource = null;

        base.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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

    private void SetQueryFilterOnDeletableEntity<T>(ModelBuilder modelBuilder) where T : DeletableEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => !x.IsDeleted && x.DeletedDate == null);
    }
}