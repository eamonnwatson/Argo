using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;

namespace Argo.Data.Repositories;

/// <summary>
/// Provides data access for <see cref="Project"/> aggregates, including their
/// related work items, activities, and RAID entries.
/// </summary>
public interface IProjectRepository : IRepository<Project, ProjectId>
{
    /// <summary>
    /// Retrieves all projects with their owner, work items, activities, and RAID
    /// entries eagerly loaded.
    /// </summary>
    Task<IReadOnlyCollection<Project>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single project with its owner, work items, activities, and RAID
    /// entries eagerly loaded.
    /// </summary>
    Task<Project?> GetByIdWithDetailsAsync(ProjectId id, CancellationToken cancellationToken = default);
}
