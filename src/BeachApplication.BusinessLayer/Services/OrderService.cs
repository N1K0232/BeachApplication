using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Collections;
using BeachApplication.Shared.Enums;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public OrderService(IApplicationDbContext applicationDbContext, IUserService userService, IMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = applicationDbContext.GetData<Entities.Order>(trackingChanges: true);
        var order = await query.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);

        if (order is not null)
        {
            var orderDetails = order.OrderDetails;
            if (orderDetails.Count > 0)
            {
                await applicationDbContext.DeleteAsync(orderDetails);
            }

            order.Status = OrderStatus.Canceled;
            await applicationDbContext.DeleteAsync(order);

            var affectedRows = await applicationDbContext.SaveAsync();
            if (affectedRows > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ClientError, ErrorMessages.DatabaseDeleteError);
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Order, id));
    }

    public async Task<Result<Order>> GetAsync(Guid id)
    {
        var query = applicationDbContext.GetData<Entities.Order>();
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
        var query = applicationDbContext.GetData<Entities.Order>();
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

    public async Task<Result<Order>> SaveAsync(SaveOrderRequest request)
    {
        var query = applicationDbContext.GetData<Entities.Order>(trackingChanges: true);

        try
        {
            var id = await CheckSaveAsync(request.OrderId);
            var dbOrder = await query.Include(o => o.OrderDetails).FirstAsync(o => o.Id == id);

            var product = await applicationDbContext.GetData<Entities.Product>(trackingChanges: true).FirstAsync(p => p.Id == request.ProductId);
            if (product.Quantity is not null && request.Quantity is not null)
            {
                if (product.Quantity < request.Quantity)
                {
                    return Result.Fail(FailureReasons.ClientError, "Not enough products");
                }

                product.Quantity -= request.Quantity;
            }

            var orderDetail = mapper.Map<Entities.OrderDetail>(request);
            orderDetail.OrderId = id;

            dbOrder.OrderDetails.Add(orderDetail);
            await applicationDbContext.SaveAsync();

            var savedOrder = mapper.Map<Order>(dbOrder);
            savedOrder.OrderDetails = mapper.Map<IEnumerable<OrderDetail>>(dbOrder.OrderDetails);

            return savedOrder;
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(FailureReasons.ClientError, ex);
        }
    }

    private async Task<Guid> CheckSaveAsync(Guid? orderId)
    {
        var exists = await CheckExistsAsync(orderId);
        if (exists)
        {
            return orderId.Value;
        }
        else
        {
            if (orderId is not null && orderId.HasValue)
            {
                throw new ArgumentException("invalid id", nameof(orderId));
            }

            var userId = await userService.GetIdAsync();
            var order = new Entities.Order
            {
                UserId = userId,
                Status = OrderStatus.New,
                OrderDate = DateTime.UtcNow.ToDateOnly(),
                OrderTime = DateTime.UtcNow.ToTimeOnly()
            };

            await applicationDbContext.InsertAsync(order);
            await applicationDbContext.SaveAsync();

            return order.Id;
        }
    }

    private async Task<bool> CheckExistsAsync(Guid? orderId)
    {
        var query = applicationDbContext.GetData<Entities.Order>();
        return await query.AnyAsync(o => o.Id == orderId);
    }

    private async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(Guid id)
    {
        var query = applicationDbContext.GetData<Entities.OrderDetail>();
        var orderDetails = await query.Where(o => o.OrderId == id).ToListAsync();

        return mapper.Map<IEnumerable<OrderDetail>>(orderDetails);
    }
}