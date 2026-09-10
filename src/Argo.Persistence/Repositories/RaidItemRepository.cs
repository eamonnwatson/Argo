using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IRaidItemRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
public class RaidItemRepository(ArgoDbContext dbContext) : IRaidItemRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public async Task<RaidItem?> GetByIdAsync(RaidItemId id, CancellationToken cancellationToken = default) =>
        await dbContext.RaidItems.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<RaidItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.RaidItems.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<RaidItem>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default) =>
        await dbContext.RaidItems
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public void Add(RaidItem entity) => dbContext.RaidItems.Add(entity);

    /// <inheritdoc />
    public void Remove(RaidItem entity) => dbContext.RaidItems.Remove(entity);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
