using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class ProductService(IApplicationDbContext context, ISqlClientCache cache, IMapper mapper) : IProductService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = context.GetData<Entities.Product>(trackingChanges: true);
        var product = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (product is not null)
        {
            await context.DeleteAsync(product);
            await context.SaveAsync();

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
    }

    public async Task<Result<Product>> GetAsync(Guid id)
    {
        var query = context.GetData<Entities.Product>().Include(p => p.Category).AsQueryable();
        var dbProduct = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (dbProduct is not null)
        {
            var product = mapper.Map<Product>(dbProduct);
            return product;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
    }

    public async Task<Result<PaginatedList<Product>>> GetListAsync(string name, string category, int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = context.GetData<Entities.Product>().Include(p => p.Category).AsQueryable();

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
        var query = context.GetData<Entities.Product>();
        var exists = await query.AnyAsync(p => p.Name == request.Name && p.Quantity == request.Quantity && p.Price == request.Price);

        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, string.Format(ErrorMessages.EntityExists, EntityNames.Product));
        }

        var categoryExists = await CategoryExistsAsync(request.CategoryId);
        if (!categoryExists)
        {
            return Result.Fail(FailureReasons.ClientError, string.Format(ErrorMessages.ItemNotFound, EntityNames.Category, request.CategoryId));
        }

        var product = mapper.Map<Entities.Product>(request);
        await context.InsertAsync(product);

        await context.SaveAsync();
        await cache.SetAsync(product, TimeSpan.FromHours(1));

        var savedProduct = mapper.Map<Product>(product);
        return savedProduct;
    }

    public async Task<Result<Product>> UpdateAsync(Guid id, SaveProductRequest request)
    {
        var query = context.GetData<Entities.Product>(ignoreQueryFilters: true, trackingChanges: true);
        var product = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (product is not null)
        {
            mapper.Map(request, product);
            await context.SaveAsync();

            var savedProduct = mapper.Map<Product>(product);
            return savedProduct;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
    }

    private async Task<bool> CategoryExistsAsync(Guid id)
    {
        var query = context.GetData<Entities.Category>();
        return await query.AnyAsync(c => c.Id == id);
    }
}