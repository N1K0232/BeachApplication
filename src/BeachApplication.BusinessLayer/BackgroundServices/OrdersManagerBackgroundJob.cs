using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.BackgroundServices;

public class OrdersManagerBackgroundJob(DataContext dataContext) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var orders = await dataContext.GetData<Order>(ignoreQueryFilters: true)
            .Include(o => o.OrderDetails)
            .Where(o => o.OrderDate < DateTime.UtcNow.ToDateOnly())
            .ToListAsync(context.CancellationToken);

        foreach (var order in orders)
        {
            if (order.OrderDetails?.Count > 0)
            {
                dataContext.Set<OrderDetail>().RemoveRange(order.OrderDetails);
            }
        }

        // by not calling the SaveAsync method i will avoid to set the query filters rule
        // this avoids to still have all the orders in the database

        dataContext.Set<Order>().RemoveRange(orders);
        await dataContext.SaveChangesAsync(context.CancellationToken);
    }
}