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

public class CategoryService(IApplicationDbContext context, ISqlClientCache cache, IMapper mapper) : ICategoryService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var category = await context.GetAsync<Entities.Category>(id);
        if (category is not null)
        {
            await context.DeleteAsync(category);
            await context.SaveAsync();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
    }

    public async Task<Result<Category>> GetAsync(Guid id)
    {
        var dbCategory = await context.GetAsync<Entities.Category>(id);
        if (dbCategory is not null)
        {
            var category = mapper.Map<Category>(dbCategory);
            return category;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
    }

    public async Task<Result<IEnumerable<Category>>> GetListAsync(string? name)
    {
        var query = context.GetData<Entities.Category>();

        if (name.HasValue())
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        var categories = await query.OrderBy(c => c.Name).ProjectTo<Category>(mapper.ConfigurationProvider).ToListAsync();
        return categories;
    }

    public async Task<Result<Category>> InsertAsync(SaveCategoryRequest request)
    {
        var query = context.GetData<Entities.Category>();
        var exists = await query.AnyAsync(c => c.Name == request.Name && c.Description == request.Description);

        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, "Cannot insert a category with the same name", "Cannot insert a category with the same name");
        }

        var category = mapper.Map<Entities.Category>(request);
        await context.InsertAsync(category);

        await context.SaveAsync();
        await cache.SetAsync(category, TimeSpan.FromHours(1));

        var savedCategory = mapper.Map<Category>(category);
        return savedCategory;
    }

    public async Task<Result<Category>> UpdateAsync(Guid id, SaveCategoryRequest request)
    {
        var query = context.GetData<Entities.Category>(trackingChanges: true);
        var category = await query.FirstOrDefaultAsync(c => c.Id == id);

        if (category is not null)
        {
            mapper.Map(request, category);
            await context.SaveAsync();

            var savedCategory = mapper.Map<Category>(category);
            return savedCategory;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
    }
}