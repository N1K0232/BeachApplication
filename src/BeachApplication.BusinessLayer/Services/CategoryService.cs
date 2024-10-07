using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class CategoryService(IApplicationDbContext db, ISqlClientCache cache, IMapper mapper) : ICategoryService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbCategory = await db.GetAsync<Entities.Category>(id);
        if (dbCategory is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
        }

        await db.DeleteAsync(dbCategory);
        await db.SaveAsync();

        if (await cache.ExistsAsync(id))
        {
            await cache.RemoveAsync(id);
        }

        return Result.Ok();
    }

    public async Task<Result<Category>> GetAsync(Guid id)
    {
        var cachedCategory = await cache.GetAsync<Entities.Category>(id);
        if (cachedCategory is not null)
        {
            return mapper.Map<Category>(cachedCategory);
        }

        var dbCategory = await db.GetAsync<Entities.Category>(id);
        if (dbCategory is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
        }

        var category = mapper.Map<Category>(dbCategory);
        await cache.RefreshAsync(id);

        return category;
    }

    public async Task<Result<IEnumerable<Category>>> GetListAsync(string name)
    {
        var query = db.GetData<Entities.Category>();

        if (name.HasValue())
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        var categories = await query.OrderBy(c => c.Name)
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .ToListAsync();

        return categories;
    }

    public async Task<Result<Category>> InsertAsync(SaveCategoryRequest request)
    {
        var query = db.GetData<Entities.Category>();
        var exists = await query.AnyAsync(c => c.Name == request.Name && c.Description == request.Description);

        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, "Cannot insert a category with the same name", "Cannot insert a category with the same name");
        }

        var dbCategory = mapper.Map<Entities.Category>(request);
        await db.InsertAsync(dbCategory);

        await cache.SetAsync(dbCategory);
        return mapper.Map<Category>(dbCategory);
    }

    public async Task<Result<Category>> UpdateAsync(Guid id, SaveCategoryRequest request)
    {
        var dbCategory = await db.GetData<Entities.Category>(trackingChanges: true).FirstOrDefaultAsync(c => c.Id == id);
        if (dbCategory is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
        }

        mapper.Map(request, dbCategory);
        await db.SaveAsync();

        await cache.UpdateAsync(dbCategory);
        return mapper.Map<Category>(dbCategory);
    }
}