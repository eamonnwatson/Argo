using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;

namespace Argo.Data.Repositories;

/// <summary>
/// Provides data access for <see cref="Activity"/> entities.
/// </summary>
public interface IActivityRepository : IRepository<Activity, ActivityId>
{
    /// <summary>
    /// Retrieves all activities belonging to the specified work item.
    /// </summary>
    Task<IReadOnlyCollection<Activity>> GetByWorkItemIdAsync(WorkItemId workItemId, CancellationToken cancellationToken = default);
}
