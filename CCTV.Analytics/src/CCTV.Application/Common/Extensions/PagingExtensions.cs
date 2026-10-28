using System.Linq.Expressions;
using CCTV.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Application.Common.Extensions;

public static class PagingExtensions
{
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<T, TDto>(
        this IQueryable<T> query,
        BaseFilterRequest request,
        Expression<Func<T, TDto>> projection,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var limit = request.Limit ?? request.PageSize ?? 10;
        if (limit <= 0) limit = 10;

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<TDto>(items, totalItems, page, limit);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        BaseFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var limit = request.Limit ?? request.PageSize ?? 10;
        if (limit <= 0) limit = 10;

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalItems, page, limit);
    }
}
