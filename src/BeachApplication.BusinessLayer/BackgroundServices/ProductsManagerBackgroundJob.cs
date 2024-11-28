using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BeachApplication.BusinessLayer.BackgroundServices;

public class ProductsManagerBackgroundJob(IDataContext dataContext) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var products = await dataContext.GetData<Product>().ToListAsync(context.CancellationToken);
        foreach (var product in products)
        {
            var item = await dataContext.GetData<CartItem>(trackingChanges: true).FirstOrDefaultAsync(c => c.ProductId == product.Id, context.CancellationToken);
            if (item is not null && product.Quantity is not null && product.Quantity < item.Quantity)
            {
                item.Quantity = product.Quantity.Value;
            }
        }

        await dataContext.SaveAsync();
    }
}