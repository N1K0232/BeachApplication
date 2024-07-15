using System.Reflection;
using BeachApplication.Authentication;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.DataAccessLayer.Entities.Common;
using BeachApplication.DataAccessLayer.Internal;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BeachApplication.DataAccessLayer;

public class ApplicationDbContext : AuthenticationDbContext, IApplicationDbContext
{
    private static readonly MethodInfo setQueryFilterOnDeletableEntity = typeof(ApplicationDbContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private readonly IEntityStore entityStore;
    private readonly ISqlClientCache cache;
    private CancellationTokenSource tokenSource;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        IEntityStore entityStore,
        ISqlClientCache cache) : base(options)
    {
        this.entityStore = entityStore;
        this.cache = cache;
        tokenSource = new CancellationTokenSource();
    }

    public async Task DeleteAsync<T>(T entity) where T : BaseEntity
    {
        await cache.RemoveAsync(entity.Id, tokenSource.Token);
        Set<T>().Remove(entity);
    }

    public async Task DeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        foreach (var entity in entities)
        {
            await cache.RemoveAsync(entity.Id, tokenSource.Token);
        }

        Set<T>().RemoveRange(entities);
    }

    public async Task<T> GetAsync<T>(Guid id) where T : BaseEntity
    {
        var entity = await GetInternalAsync<T>(id, tokenSource.Token);
        if (entity is not null)
        {
            await GenerateSecurityOptionsAsync(entity, tokenSource.Token);
            await SaveChangesAsync(true, tokenSource.Token);
        }

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
        await GenerateSecurityOptionsAsync(entity, tokenSource.Token);
        await Set<T>().AddAsync(entity, tokenSource.Token);
        await cache.SetAsync(entity, TimeSpan.FromHours(1), tokenSource.Token);
    }

    public async Task<int> SaveAsync()
    {
        var entries = GetEntries(typeof(BaseEntity));

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
                await GenerateSecurityOptionsAsync(entity, tokenSource.Token);

                await cache.RemoveAsync(entity.Id, tokenSource.Token);
                await cache.SetAsync(entity, TimeSpan.FromHours(1), tokenSource.Token);
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
        builder.Entity<T>().HasQueryFilter(x => !x.IsDeleted && x.DeletedDate == null);
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
        var entries = ChangeTracker.Entries();
        return entries.Where(e => entityType.IsAssignableFrom(e.Entity.GetType()));
    }

    private async Task<T> GetInternalAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var cachedEntity = await cache.GetAsync<T>(id, cancellationToken);
        if (cachedEntity is not null)
        {
            return cachedEntity;
        }

        var entity = await Set<T>().FindAsync([id], cancellationToken);
        return entity;
    }

    private async Task GenerateSecurityOptionsAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        await entityStore.GenerateConcurrencyStampAsync(entity, cancellationToken);
        await entityStore.GenerateSecurityStampAsync(entity, cancellationToken);
    }
}