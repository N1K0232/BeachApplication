using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.Shared.Collections;
using BeachApplication.Shared.Enums;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class OrderService(IApplicationDbContext context, ISqlClientCache cache, IUserService userService, IMapper mapper) : IOrderService
{
    public async Task<Result> AddOrderDetailAsync(SaveOrderRequest request)
    {
        var dbOrder = await context.GetData<Entities.Order>().FirstAsync(o => o.Id == request.OrderId);
        var dbProduct = await context.GetData<Entities.Product>(trackingChanges: true).FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (dbProduct is null)
        {
            return Result.Fail(FailureReasons.ClientError, string.Format(ErrorMessages.ItemNotFound, EntityNames.Product, request.ProductId));
        }

        if (dbProduct.Quantity is not null)
        {
            dbProduct.Quantity -= request.Quantity;
        }

        var orderDetail = mapper.Map<Entities.OrderDetail>(request);
        orderDetail.Price = Convert.ToDecimal(dbProduct.Price * request.Quantity);

        await context.InsertAsync(orderDetail);
        await context.SaveAsync();

        await cache.SetAsync(orderDetail, TimeSpan.FromHours(1));
        return Result.Ok();
    }

    public async Task<Result<Order>> CreateAsync()
    {
        var userId = await userService.GetIdAsync();
        var dbOrder = new Entities.Order
        {
            UserId = userId,
            Status = OrderStatus.New,
            OrderDate = DateTime.UtcNow.ToDateOnly(),
            OrderTime = DateTime.UtcNow.ToTimeOnly(),
        };

        await context.InsertAsync(dbOrder);
        await context.SaveAsync();

        await cache.SetAsync(dbOrder, TimeSpan.FromHours(1));
        return mapper.Map<Order>(dbOrder);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = context.GetData<Entities.Order>(trackingChanges: true);
        var order = await query.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);

        if (order is not null)
        {
            var orderDetails = order.OrderDetails;
            if (orderDetails.Count > 0)
            {
                await context.DeleteAsync(orderDetails);
            }

            order.Status = OrderStatus.Canceled;
            await context.DeleteAsync(order);

            await context.SaveAsync();
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Order, id));
    }

    public async Task<Result<Order>> GetAsync(Guid id)
    {
        var query = context.GetData<Entities.Order>();
        var dbOrder = await query.FirstOrDefaultAsync(o => o.Id == id);

        if (dbOrder is not null)
        {
            var order = mapper.Map<Order>(dbOrder);
            order.OrderDetails = await GetOrderDetailsAsync(id);

            return order;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Order, id));
    }

    public async Task<Result<ListResult<Order>>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = context.GetData<Entities.Order>();
        var totalCount = await query.LongCountAsync();
        var totalPages = await query.TotalPagesAsync(itemsPerPage);
        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);
        var dbOrders = await query.OrderBy(orderBy).ToListAsync(pageIndex, itemsPerPage);

        var orders = mapper.Map<IEnumerable<Order>>(dbOrders).Take(itemsPerPage);
        await orders.ForEachAsync(async (order) =>
        {
            order.OrderDetails = await GetOrderDetailsAsync(order.Id);
        });

        return new ListResult<Order>(orders, totalCount, totalPages, hasNextPage);
    }

    private async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(Guid id)
    {
        var query = context.GetData<Entities.OrderDetail>();
        var orderDetails = await query.Where(o => o.OrderId == id).ToListAsync();

        return mapper.Map<IEnumerable<OrderDetail>>(orderDetails);
    }
}