using System.Collections.Concurrent;
using CCTV.Application.Common.Interfaces;
using CCTV.Domain.Shared;
using CCTV.Persistence.Contexts;

namespace CCTV.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CCTVAnalyticsDbContext _context;
    private readonly ConcurrentDictionary<string, object> _repositories = new();
    private bool _disposed;

    public UnitOfWork(CCTVAnalyticsDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var typeName = typeof(TEntity).Name;

        return (IGenericRepository<TEntity>)_repositories.GetOrAdd(typeName, _ => new GenericRepository<TEntity>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
