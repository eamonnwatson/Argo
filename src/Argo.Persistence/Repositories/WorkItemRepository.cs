using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IWorkItemRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
public class WorkItemRepository(ArgoDbContext dbContext) : IWorkItemRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public async Task<WorkItem?> GetByIdAsync(WorkItemId id, CancellationToken cancellationToken = default) =>
        await dbContext.WorkItems.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<WorkItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.WorkItems.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<WorkItem>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default) =>
        await dbContext.WorkItems
            .Where(w => w.ProjectId == projectId)
            .Include(w => w.Activities)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public void Add(WorkItem entity) => dbContext.WorkItems.Add(entity);

    /// <inheritdoc />
    public void Remove(WorkItem entity) => dbContext.WorkItems.Remove(entity);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
