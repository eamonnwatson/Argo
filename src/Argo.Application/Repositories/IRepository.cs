using FluentResults;

namespace Argo.Application.Repositories;

/// <summary>
/// Provides basic asynchronous CRUD operations shared by all Argo persistence repositories.
/// </summary>
/// <typeparam name="TEntity">The aggregate/entity type managed by the repository.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type of <typeparamref name="TEntity"/>.</typeparam>
public interface IRepository<TEntity, in TId>
    where TEntity : class
{
    /// <summary>
    /// Retrieves an entity by its identifier, or <see langword="null"/> if it does not exist.
    /// </summary>
    Task<Result<TEntity?>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities tracked by the underlying store.
    /// </summary>
    Task<Result<IReadOnlyCollection<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the underlying store.
    /// </summary>
    void Add(TEntity entity);

    /// <summary>
    /// Removes an existing entity from the underlying store.
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Persists all pending changes made through this repository.
    /// </summary>
    Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default);
}
