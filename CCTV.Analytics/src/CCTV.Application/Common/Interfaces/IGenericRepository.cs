using System.Linq.Expressions;
using CCTV.Domain.Shared;

namespace CCTV.Application.Common.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Standard Offset Pagination (SKIP/TAKE) for low-volume administrative models (Users, Sites, Cameras).
    /// </summary>
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// High-Performance Keyset / Cursor Pagination for large time-series telemetry data (Events, Tracks, Heartbeats).
    /// Prevents O(N) OFFSET degradation in SQL Server.
    /// </summary>
    Task<IReadOnlyList<T>> GetKeysetPagedAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, DateTime>> timePropertySelector,
        DateTime? lastTimestamp,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}
