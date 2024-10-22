using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class OrderService(IApplicationDbContext db, IUserService userService, IMapper mapper) : IOrderService
{
    public async Task<Result> CancelAsync(Guid id)
    {
        var dbOrder = await db.GetData<Entities.Order>(trackingChanges: true).Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
        if (dbOrder is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {id}");
        }

        if (dbOrder.OrderDetails?.Count > 0)
        {
            await db.DeleteAsync(dbOrder.OrderDetails);
        }

        await db.DeleteAsync(dbOrder);
        await db.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Order>> GetAsync()
    {
        var userId = await userService.GetIdAsync();
        var dbOrder = await db.GetData<Entities.Order>().FirstOrDefaultAsync(o => o.UserId == userId);

        if (dbOrder is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "You have no orders. To create one, start by adding products to your cart");
        }

        var order = mapper.Map<Order>(dbOrder);
        order.OrderDetails = await GetOrderDetailsAsync(order.Id);

        return order;
    }

    public async Task<Result<PaginatedList<Order>>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = db.GetData<Entities.Order>();
        var totalCount = await query.CountAsync();

        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);
        var dbOrders = await query.OrderBy(orderBy).ToListAsync(pageIndex, itemsPerPage);

        var orders = mapper.Map<IEnumerable<Order>>(dbOrders).Take(itemsPerPage);
        await orders.ForEachAsync(async order =>
        {
            order.OrderDetails = await GetOrderDetailsAsync(order.Id);
        });

        return new PaginatedList<Order>(orders, totalCount, hasNextPage);
    }

    private async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(Guid id)
    {
        var dbOrderDetails = await db.GetData<Entities.OrderDetail>()
            .Where(o => o.OrderId == id)
            .ToListAsync();

        return mapper.Map<IEnumerable<OrderDetail>>(dbOrderDetails);
    }
}