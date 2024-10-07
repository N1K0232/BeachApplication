using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.Shared.Enums;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class OrderService(IApplicationDbContext db, ISqlClientCache cache, IUserService userService, IMapper mapper) : IOrderService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = db.GetData<Entities.Order>(trackingChanges: true);
        var order = await query.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);

        if (order is not null)
        {
            var orderDetails = order.OrderDetails;
            if (orderDetails?.Count > 0)
            {
                await db.DeleteAsync(orderDetails);
            }

            order.Status = OrderStatus.Canceled;
            await db.DeleteAsync(order);

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Order, id));
    }

    public async Task<Result<Order>> GetAsync(Guid id)
    {
        var query = db.GetData<Entities.Order>();
        var dbOrder = await query.FirstOrDefaultAsync(o => o.Id == id);

        if (dbOrder is not null)
        {
            var order = mapper.Map<Order>(dbOrder);
            order.OrderDetails = await LoadOrderDetailsAsync(id);

            return order;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Order, id));
    }

    public async Task<Result<PaginatedList<Order>>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = db.GetData<Entities.Order>();
        var totalCount = await query.CountAsync();

        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);
        var dbOrders = await query.OrderBy(orderBy).ToListAsync(pageIndex, itemsPerPage);

        var orders = mapper.Map<IEnumerable<Order>>(dbOrders).Take(itemsPerPage);
        await orders.ForEachAsync(async (order) =>
        {
            order.OrderDetails = await LoadOrderDetailsAsync(order.Id);
        });

        return new PaginatedList<Order>(orders, totalCount, hasNextPage);
    }

    public async Task<Result<Order>> SaveAsync(SaveOrderRequest request)
    {
        var dbOrder = await db.GetAsync<Entities.Order>(request.Id);
        if (dbOrder is null)
        {
            var userId = await userService.GetIdAsync();
            dbOrder = new Entities.Order
            {
                Id = request.Id,
                UserId = userId,
                Status = OrderStatus.New,
                OrderDate = DateTime.UtcNow.ToDateOnly(),
                OrderTime = DateTime.UtcNow.ToTimeOnly()
            };

            await db.InsertAsync(dbOrder);
            await cache.SetAsync(dbOrder);
            await db.SaveAsync();
        }

        var dbProduct = await db.GetData<Entities.Product>(trackingChanges: true).FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (dbProduct is null)
        {
            return Result.Fail(FailureReasons.ClientError, "Product not found");
        }

        if (dbProduct.Quantity is not null)
        {
            if (dbProduct.Quantity > request.Quantity)
            {
                dbProduct.Quantity -= request.Quantity;
            }
            else
            {
                return Result.Fail(FailureReasons.ClientError, "You specified too many products");
            }
        }

        var details = mapper.Map<Entities.OrderDetail>(request);
        await db.InsertAsync(details);

        await db.SaveAsync();
        await cache.SetAsync(details);

        return mapper.Map<Order>(dbOrder);
    }

    private async Task<IEnumerable<OrderDetail>> LoadOrderDetailsAsync(Guid id)
    {
        var orderDetails = await db.GetData<Entities.OrderDetail>()
            .Where(o => o.OrderId == id)
            .ToListAsync();

        return mapper.Map<IEnumerable<OrderDetail>>(orderDetails);
    }
}