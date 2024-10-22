using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BeachApplication.BusinessLayer.BackgroundServices;

public class ProductsManagerBackgroundJob(IApplicationDbContext db) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var products = await db.GetData<Product>().ToListAsync(context.CancellationToken);
        foreach (var product in products)
        {
            var item = await db.GetData<CartItem>(trackingChanges: true).FirstOrDefaultAsync(c => c.ProductId == product.Id, context.CancellationToken);
            if (item is not null && product.Quantity is not null && product.Quantity < item.Quantity)
            {
                item.Quantity = product.Quantity.Value;
            }
        }

        await db.SaveAsync();
    }
}