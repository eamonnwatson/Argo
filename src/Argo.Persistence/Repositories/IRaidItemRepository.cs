using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;

namespace Argo.Data.Repositories;

/// <summary>
/// Provides data access for <see cref="RaidItem"/> entities.
/// </summary>
public interface IRaidItemRepository : IRepository<RaidItem, RaidItemId>
{
    /// <summary>
    /// Retrieves all RAID entries belonging to the specified project.
    /// </summary>
    Task<IReadOnlyCollection<RaidItem>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default);
}
