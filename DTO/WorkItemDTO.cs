namespace Argo.DTO;

public record WorkItemDTO(string Id, string ProjectId, string Title, string Owner, string Status, DateOnly DueDate, string Dependency, string Purpose, string Participants, string RequiredInputs, string Milestone, string DefinitionOfDone);

public record WorkItemCreateDTO(string ProjectId, string Title, string Owner, string Status, DateOnly DueDate, string Dependency, string Purpose, string Participants, string RequiredInputs, string Milestone, string DefinitionOfDone);
