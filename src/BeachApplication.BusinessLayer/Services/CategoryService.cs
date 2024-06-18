using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly IMapper mapper;

    public CategoryService(IApplicationDbContext applicationDbContext, IMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var category = await applicationDbContext.GetAsync<Entities.Category>(id);
        if (category is not null)
        {
            await applicationDbContext.DeleteAsync(category);
            var affectedRows = await applicationDbContext.SaveAsync();

            if (affectedRows > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ClientError, "No category deleted", "An error occurred. No category was deleted");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found", $"No category found with id {id}");
    }

    public async Task<Result<Category>> GetAsync(Guid id)
    {
        var dbCategory = await applicationDbContext.GetAsync<Entities.Category>(id);
        if (dbCategory is not null)
        {
            var category = mapper.Map<Category>(dbCategory);
            return category;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found", $"No category found with id {id}");
    }

    public async Task<Result<IEnumerable<Category>>> GetListAsync(string name, string description)
    {
        var query = applicationDbContext.GetData<Entities.Category>();

        if (name.HasValue())
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (description.HasValue())
        {
            query = query.Where(c => c.Description.Contains(description));
        }

        var categories = await query.OrderBy(c => c.Name).ProjectTo<Category>(mapper.ConfigurationProvider).ToListAsync();
        return categories;
    }

    public async Task<Result<Category>> InsertAsync(SaveCategoryRequest request)
    {
        var query = applicationDbContext.GetData<Entities.Category>();
        var exists = await query.AnyAsync(c => c.Name == request.Name && c.Description == request.Description);

        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, "Cannot insert a category with the same name", "Cannot insert a category with the same name");
        }

        var category = mapper.Map<Entities.Category>(request);
        await applicationDbContext.InsertAsync(category);

        var affectedRows = await applicationDbContext.SaveAsync();
        if (affectedRows > 0)
        {
            var savedCategory = mapper.Map<Category>(category);
            return savedCategory;
        }

        return Result.Fail(FailureReasons.ClientError, "No category added", $"An error occurred. No category added");
    }

    public async Task<Result<Category>> UpdateAsync(Guid id, SaveCategoryRequest request)
    {
        var query = applicationDbContext.GetData<Entities.Category>(trackingChanges: true);
        var category = await query.FirstOrDefaultAsync(c => c.Id == id);

        if (category is not null)
        {
            mapper.Map(request, category);
            var affectedRows = await applicationDbContext.SaveAsync();

            if (affectedRows > 0)
            {
                var savedCategory = mapper.Map<Category>(category);
                return savedCategory;
            }

            return Result.Fail(FailureReasons.ClientError, "No category updated", "An error occurred. No category was updated");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found", $"No category found with id {id}");
    }
}