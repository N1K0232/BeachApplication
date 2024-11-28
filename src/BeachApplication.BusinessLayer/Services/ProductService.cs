using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class ProductService(IDataContext dataContext, IMapper mapper) : IProductService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbProduct = await dataContext.GetData<Entities.Product>(trackingChanges: true).FirstOrDefaultAsync(p => p.Id == id);
        if (dbProduct is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
        }

        await dataContext.DeleteAsync(dbProduct);
        await dataContext.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Product>> GetAsync(Guid id)
    {
        var query = dataContext.GetData<Entities.Product>().Include(p => p.Category).AsQueryable();
        var dbProduct = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (dbProduct is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
        }

        var product = mapper.Map<Product>(dbProduct);
        return product;
    }

    public async Task<Result<PaginatedList<Product>>> GetListAsync(string name, string category, int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = dataContext.GetData<Entities.Product>().Include(p => p.Category).AsQueryable();

        if (name.HasValue())
        {
            query = query.Where(p => p.Name.Contains(name));
        }

        if (category.HasValue())
        {
            query = query.Where(p => p.Category.Name.Contains(category));
        }

        var totalCount = await query.CountAsync();
        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);

        var dbProducts = await query.OrderBy(orderBy).ToListAsync(pageIndex, itemsPerPage);
        var products = mapper.Map<IEnumerable<Product>>(dbProducts).Take(itemsPerPage);

        return new PaginatedList<Product>(products, totalCount, hasNextPage);
    }

    public async Task<Result<Product>> InsertAsync(SaveProductRequest request)
    {
        var exists = await ExistsAsync(request.Name, request.Description);
        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, string.Format(ErrorMessages.EntityExists, EntityNames.Product));
        }

        var category = await GetCategoryAsync(request.Category);
        if (category is null)
        {
            return Result.Fail(FailureReasons.ClientError, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, request.Category));
        }

        var dbProduct = mapper.Map<Entities.Product>(request);
        dbProduct.CategoryId = category.Id;

        await dataContext.InsertAsync(dbProduct);
        await dataContext.SaveAsync();

        return mapper.Map<Product>(dbProduct);
    }

    public async Task<Result<Product>> UpdateAsync(Guid id, SaveProductRequest request)
    {
        var query = dataContext.GetData<Entities.Product>(true, true);
        var dbProduct = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (dbProduct is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
        }

        var exists = await ExistsAsync(request.Name, request.Description);
        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, string.Format(ErrorMessages.EntityExists, EntityNames.Product));
        }

        var category = await GetCategoryAsync(request.Category);
        if (category is null)
        {
            return Result.Fail(FailureReasons.ClientError, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, request.Category));
        }

        mapper.Map(request, dbProduct);
        dbProduct.CategoryId = category.Id;

        await dataContext.SaveAsync();
        return mapper.Map<Product>(dbProduct);
    }

    private async Task<bool> ExistsAsync(string name, string description)
    {
        var query = dataContext.GetData<Entities.Product>();
        return await query.AnyAsync(p => p.Name == name && p.Description == description);
    }

    private async Task<Category> GetCategoryAsync(string name)
    {
        var category = await dataContext.GetData<Entities.Category>().FirstOrDefaultAsync(c => c.Name == name);
        return mapper.Map<Category>(category);
    }
}