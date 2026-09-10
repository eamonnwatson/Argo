using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Argo.Persistence.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IUserRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
public class UserRepository(ArgoDbContext dbContext) : BaseRepository, IUserRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public Task<Result<User?>> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<User>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<User>)await dbContext.Users.OrderBy(u => u.DisplayName).ToListAsync(cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<User>>> GetAllAsync(bool projectManagersOnly, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () =>
        {
            var query = dbContext.Users.AsNoTracking().AsQueryable();

            if (projectManagersOnly)
                query = query.Where(u => u.IsProjectManager);

            return (IReadOnlyCollection<User>)await query
                .OrderBy(u => u.DisplayName)
                .ToListAsync(cancellationToken);
        });
    }

    /// <inheritdoc />
    public void Add(User entity) => dbContext.Users.Add(entity);

    /// <inheritdoc />
    public void Remove(User entity) => dbContext.Users.Remove(entity);

    /// <inheritdoc />
    public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.SaveChangesAsync(cancellationToken));
}
