namespace Argo.DTO;

public record ProjectDTO(string Id, string Name, string Owner, string Status, string Health, string Priority, string Objective, string NextMilestone, DateOnly TargetDate, string SourceRequestId, DateTime SubmittedAt);

public record ProjectCreateDTO(string Name, string Owner, string Status, string Health, string Priority, string Objective, string NextMilestone, DateOnly TargetDate);
