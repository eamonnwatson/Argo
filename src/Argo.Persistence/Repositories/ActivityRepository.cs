using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IActivityRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
public class ActivityRepository(ArgoDbContext dbContext) : IActivityRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Activity?> GetByIdAsync(ActivityId id, CancellationToken cancellationToken = default) =>
        await dbContext.Activities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Activity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Activities.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Activity>> GetByWorkItemIdAsync(WorkItemId workItemId, CancellationToken cancellationToken = default) =>
        await dbContext.Activities
            .Where(a => a.WorkItemId == workItemId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public void Add(Activity entity) => dbContext.Activities.Add(entity);

    /// <inheritdoc />
    public void Remove(Activity entity) => dbContext.Activities.Remove(entity);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
