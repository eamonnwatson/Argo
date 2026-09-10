using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Argo.Persistence.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IActivityRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
public class ActivityRepository(ArgoDbContext dbContext) : BaseRepository, IActivityRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public Task<Result<Activity?>> GetByIdAsync(ActivityId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Activities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<Activity>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<Activity>)await dbContext.Activities.ToListAsync(cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<Activity>>> GetByWorkItemIdAsync(WorkItemId workItemId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<Activity>)await dbContext.Activities
            .Where(a => a.WorkItemId == workItemId)
            .ToListAsync(cancellationToken));

    /// <inheritdoc />
    public void Add(Activity entity) => dbContext.Activities.Add(entity);

    /// <inheritdoc />
    public void Remove(Activity entity) => dbContext.Activities.Remove(entity);

    /// <inheritdoc />
    public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.SaveChangesAsync(cancellationToken));
}
