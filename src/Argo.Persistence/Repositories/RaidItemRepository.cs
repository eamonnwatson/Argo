using Argo.Application.Repositories;
using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Argo.Persistence.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IRaidItemRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
internal class RaidItemRepository(ArgoDbContext dbContext) : BaseRepository, IRaidItemRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public Task<Result<RaidItem?>> GetByIdAsync(RaidItemId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.RaidItems.FirstOrDefaultAsync(r => r.Id == id, cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<RaidItem>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<RaidItem>)await dbContext.RaidItems.ToListAsync(cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<RaidItem>>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<RaidItem>)await dbContext.RaidItems
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(cancellationToken));

    /// <inheritdoc />
    public void Add(RaidItem entity) => dbContext.RaidItems.Add(entity);

    /// <inheritdoc />
    public void Remove(RaidItem entity) => dbContext.RaidItems.Remove(entity);

    /// <inheritdoc />
    public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.SaveChangesAsync(cancellationToken));
}
