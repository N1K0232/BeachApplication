using AutoMapper;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Enums;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class CartService(IApplicationDbContext db, IUserService userService, IMapper mapper) : ICartService
{
    public async Task<Result<Order>> ConfirmAsync(Guid id)
    {
        var cart = await db.GetData<Entities.Cart>().Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (cart is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No cart found with id {id}");
        }

        if (cart.Items?.Count == 0)
        {
            return Result.Fail(FailureReasons.ClientError, "Unable to create the order. No items in the cart");
        }

        var orderId = await CreateOrderAsync();
        var dbOrder = await db.GetData<Entities.Order>().Include(o => o.OrderDetails).FirstAsync(o => o.Id == id);

        dbOrder.OrderDetails ??= [];
        foreach (var item in cart.Items)
        {
            var orderDetail = new Entities.OrderDetail
            {
                ProductId = item.ProductId,
                Price = item.Price,
                Quantity = item.Quantity
            };

            dbOrder.OrderDetails.Add(orderDetail);
        }

        await db.SaveAsync();
        return mapper.Map<Order>(dbOrder);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbCart = await db.GetData<Entities.Cart>().Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (dbCart is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No cart found with id {id}");
        }

        if (dbCart.Items.Count > 0)
        {
            await db.DeleteAsync(dbCart.Items);
        }

        await db.DeleteAsync(dbCart);
        await db.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Cart>> GetAsync()
    {
        var userId = await userService.GetIdAsync();
        var dbCart = await db.GetData<Entities.Cart>().Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);

        if (dbCart is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No cart found");
        }

        var cart = mapper.Map<Cart>(dbCart);
        return cart;
    }

    public async Task<Result> RemoveItemAsync(Guid id, Guid itemId)
    {
        var item = await db.GetData<Entities.CartItem>().FirstOrDefaultAsync(c => c.CartId == id && c.Id == itemId);
        if (item is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No item found with id {id}");
        }

        await db.DeleteAsync(item);
        await db.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Cart>> SaveAsync(SaveCartRequest request)
    {
        var cart = await GetOrCreateCartAsync(request.Id);
        var product = await db.GetData<Entities.Product>().FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product is null)
        {
            return Result.Fail(FailureReasons.ClientError, "No product found with the specified id");
        }

        var dbCartItem = new Entities.CartItem
        {
            CartId = request.Id,
            ProductId = product.Id,
        };

        if (product.Quantity is not null)
        {
            if (product.Quantity < request.Quantity)
            {
                dbCartItem.Quantity = product.Quantity.Value;
            }
            else
            {
                dbCartItem.Quantity = request.Quantity;
            }
        }

        await db.InsertAsync(dbCartItem);
        await db.SaveAsync();

        var cartItem = mapper.Map<CartItem>(dbCartItem);
        cart.Items.Add(cartItem);

        return cart;
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid id)
    {
        var dbCart = await db.GetData<Entities.Cart>().FirstOrDefaultAsync(c => c.Id == id);
        if (dbCart is null)
        {
            dbCart = new Entities.Cart
            {
                Id = id,
                UserId = await userService.GetIdAsync()
            };

            await db.InsertAsync(dbCart);
            await db.SaveAsync();
        }

        var cart = mapper.Map<Cart>(dbCart);
        cart.Items ??= [];

        return cart;
    }

    private async Task<Guid> CreateOrderAsync()
    {
        var dbOrder = new Entities.Order
        {
            UserId = await userService.GetIdAsync(),
            OrderDate = DateTime.UtcNow.ToDateOnly(),
            OrderTime = DateTime.UtcNow.ToTimeOnly(),
            Status = OrderStatus.New
        };

        await db.InsertAsync(dbOrder);
        await db.SaveAsync();

        return dbOrder.Id;
    }
}