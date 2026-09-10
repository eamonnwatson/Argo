using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using FluentResults;

namespace Argo.Data.Repositories;

/// <summary>
/// Provides data access for <see cref="WorkItem"/> entities.
/// </summary>
public interface IWorkItemRepository : IRepository<WorkItem, WorkItemId>
{
    /// <summary>
    /// Retrieves all work items belonging to the specified project.
    /// </summary>
    Task<Result<IReadOnlyCollection<WorkItem>>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default);
}
