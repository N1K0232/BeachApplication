using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class CategoryService(IApplicationDbContext db, IMapper mapper) : ICategoryService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var category = await db.GetAsync<Entities.Category>(id);
        if (category is not null)
        {
            await db.DeleteAsync(category);
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
    }

    public async Task<Result<Category>> GetAsync(Guid id)
    {
        var dbCategory = await db.GetAsync<Entities.Category>(id);
        if (dbCategory is not null)
        {
            var category = mapper.Map<Category>(dbCategory);
            return category;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
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

        var category = mapper.Map<Entities.Category>(request);
        await db.InsertAsync(category);

        return mapper.Map<Category>(category);
    }

    public async Task<Result<Category>> UpdateAsync(Guid id, SaveCategoryRequest request)
    {
        var query = db.GetData<Entities.Category>(trackingChanges: true);
        var category = await query.FirstOrDefaultAsync(c => c.Id == id);

        if (category is not null)
        {
            mapper.Map(request, category);
            await db.UpdateAsync(category);

            return mapper.Map<Category>(category);
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, id));
    }
}