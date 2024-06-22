using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Collections;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly IMapper mapper;

    public ProductService(IApplicationDbContext applicationDbContext, IMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = applicationDbContext.GetData<Entities.Product>(trackingChanges: true);
        var product = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (product is not null)
        {
            await applicationDbContext.DeleteAsync(product);
            var affectedRows = await applicationDbContext.SaveAsync();

            if (affectedRows > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ClientError, ErrorMessages.DatabaseDeleteError);
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
    }

    public async Task<Result<Product>> GetAsync(Guid id)
    {
        var query = applicationDbContext.GetData<Entities.Product>().Include(p => p.Category).AsQueryable();
        var dbProduct = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (dbProduct is not null)
        {
            var product = mapper.Map<Product>(dbProduct);
            return product;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
    }

    public async Task<Result<ListResult<Product>>> GetListAsync(string name, string category, int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = applicationDbContext.GetData<Entities.Product>().Include(p => p.Category).AsQueryable();

        if (name.HasValue())
        {
            query = query.Where(p => p.Name.Contains(name));
        }

        if (category.HasValue())
        {
            query = query.Where(p => p.Category.Name.Contains(category));
        }

        var totalCount = await query.LongCountAsync();
        var totalPages = await query.TotalPagesAsync(itemsPerPage);
        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);
        var dbProducts = await query.OrderBy(orderBy).ToListAsync(pageIndex, itemsPerPage);

        var products = mapper.Map<IEnumerable<Product>>(dbProducts).Take(itemsPerPage);
        return new ListResult<Product>(products, totalCount, totalPages, hasNextPage);
    }

    public async Task<Result<Product>> InsertAsync(SaveProductRequest request)
    {
        var query = applicationDbContext.GetData<Entities.Product>();
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
        await applicationDbContext.InsertAsync(product);

        var affectedRows = await applicationDbContext.SaveAsync();
        if (affectedRows > 0)
        {
            var savedProduct = mapper.Map<Product>(product);
            return savedProduct;
        }

        return Result.Fail(FailureReasons.ClientError, ErrorMessages.DatabaseInsertError);
    }

    public async Task<Result<Product>> UpdateAsync(Guid id, SaveProductRequest request)
    {
        var query = applicationDbContext.GetData<Entities.Product>(ignoreQueryFilters: true, trackingChanges: true);
        var product = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (product is not null)
        {
            mapper.Map(request, product);
            var affectedRows = await applicationDbContext.SaveAsync();

            if (affectedRows > 0)
            {
                var savedProduct = mapper.Map<Product>(product);
                return savedProduct;
            }

            return Result.Fail(FailureReasons.ClientError, ErrorMessages.DatabaseUpdateError);
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, id));
    }

    private async Task<bool> CategoryExistsAsync(Guid id)
    {
        var query = applicationDbContext.GetData<Entities.Category>();
        return await query.AnyAsync(c => c.Id == id);
    }
}