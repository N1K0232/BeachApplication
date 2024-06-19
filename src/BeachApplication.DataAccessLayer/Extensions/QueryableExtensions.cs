using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.DataAccessLayer.Extensions;

public static class QueryableExtensions
{
    public static async Task<bool> HasNextPageAsync<T>(this IQueryable<T> source, int pageIndex, int itemsPerPage, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var hasItems = await source.AnyAsync(cancellationToken);
        if (!hasItems)
        {
            return false;
        }

        var skip = pageIndex * itemsPerPage;
        var take = itemsPerPage + 1;

        var list = await source.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return list.Count > itemsPerPage;
    }

    public static async Task<int> TotalPagesAsync<T>(this IQueryable<T> source, int itemsPerPage, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var hasItems = await source.AnyAsync(cancellationToken);
        if (!hasItems)
        {
            return 0;
        }

        var totalCount = await source.LongCountAsync(cancellationToken);
        var totalPages = Convert.ToInt32(totalCount / itemsPerPage);

        if ((totalPages % itemsPerPage) > 0)
        {
            totalPages++;
        }

        return totalPages;
    }

    public static async Task<IList<T>> ToListAsync<T>(this IQueryable<T> source, int pageIndex, int itemsPerPage, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var hasItems = await source.AnyAsync(cancellationToken);
        if (!hasItems)
        {
            return Enumerable.Empty<T>().ToList();
        }

        var skip = pageIndex * itemsPerPage;
        var take = itemsPerPage + 1;

        return await source.Skip(skip).Take(take).ToListAsync(cancellationToken);
    }
}