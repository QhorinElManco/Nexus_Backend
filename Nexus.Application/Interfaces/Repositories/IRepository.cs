namespace Nexus.Application.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    public Task<T> AddAsync(T entity, CancellationToken ct = default);
    public Task UpdateAsync(T entity, CancellationToken ct = default);
    public Task DeleteAsync(long id, CancellationToken ct = default);
}
