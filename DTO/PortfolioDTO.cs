namespace Argo.DTO;

public record PortfolioDTO(IReadOnlyCollection<ProjectDTO> Projects, IReadOnlyCollection<WorkItemDTO> WorkItems, IReadOnlyCollection<ActivityDTO> Activities, IReadOnlyCollection<RaidItemDTO> RaidItems);
