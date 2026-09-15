using Argo.Application.Repositories;
using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Argo.Persistence.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Argo.Persistence.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IWorkItemRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
internal class WorkItemRepository(ArgoDbContext dbContext) : BaseRepository, IWorkItemRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public Task<Result<WorkItem?>> GetByIdAsync(WorkItemId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.WorkItems.FirstOrDefaultAsync(w => w.Id == id, cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<WorkItem>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<WorkItem>)await dbContext.WorkItems.ToListAsync(cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<WorkItem>>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<WorkItem>)await dbContext.WorkItems
            .Where(w => w.ProjectId == projectId)
            .Include(w => w.Activities)
            .ToListAsync(cancellationToken));

    /// <inheritdoc />
    public void Add(WorkItem entity) => dbContext.WorkItems.Add(entity);

    /// <inheritdoc />
    public void Remove(WorkItem entity) => dbContext.WorkItems.Remove(entity);

    /// <inheritdoc />
    public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.SaveChangesAsync(cancellationToken));
}
