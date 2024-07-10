using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Diagnostics.BackgroundJobs;

public class OrdersManagerBackgroundJob : IJob
{
    private readonly ApplicationDbContext applicationDbContext;

    public OrdersManagerBackgroundJob(ApplicationDbContext applicationDbContext)
    {
        this.applicationDbContext = applicationDbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var orders = await applicationDbContext.GetData<Order>(ignoreQueryFilters: true)
            .Include(o => o.OrderDetails)
            .Where(o => o.OrderDate < DateTime.UtcNow.ToDateOnly())
            .ToListAsync(context.CancellationToken);

        foreach (var order in orders)
        {
            applicationDbContext.Set<OrderDetail>().RemoveRange(order.OrderDetails);
        }

        // by not calling the SaveAsync method i will avoid to set the query filters rule
        // this avoids to still have all the orders in the database

        applicationDbContext.Set<Order>().RemoveRange(orders);
        await applicationDbContext.SaveChangesAsync(context.CancellationToken);
    }
}