namespace Argo.DTO;

/// <summary>
/// Represents the portfolio payload returned to populate the main dashboard views.
/// </summary>
/// <param name="Projects">The projects included in the portfolio response.</param>
/// <param name="WorkItems">The work items associated with portfolio projects.</param>
/// <param name="Activities">The activity entries associated with work items.</param>
/// <param name="RaidItems">The RAID entries associated with projects.</param>
public record PortfolioDTO(IReadOnlyCollection<ProjectDTO> Projects, IReadOnlyCollection<WorkItemDTO> WorkItems, IReadOnlyCollection<ActivityDTO> Activities, IReadOnlyCollection<RaidItemDTO> RaidItems);
