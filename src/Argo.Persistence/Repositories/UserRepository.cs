using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IUserRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
public class UserRepository(ArgoDbContext dbContext) : IUserRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Users.OrderBy(u => u.DisplayName).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<User>> GetAllAsync(bool projectManagersOnly, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.AsNoTracking().AsQueryable();

        if (projectManagersOnly)
            query = query.Where(u => u.IsProjectManager);

        return await query
            .OrderBy(u => u.DisplayName)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Add(User entity) => dbContext.Users.Add(entity);

    /// <inheritdoc />
    public void Remove(User entity) => dbContext.Users.Remove(entity);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
