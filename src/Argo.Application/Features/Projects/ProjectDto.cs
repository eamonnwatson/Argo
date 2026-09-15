namespace Argo.Application.Features.Projects;

public record ProjectDto(string Id, string Name, string Owner, string Status, string Health, string Priority, string Objective, string NextMilestone, DateOnly TargetDate, string SourceRequestId, string IntakeDetails);