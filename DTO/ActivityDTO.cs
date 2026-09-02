namespace Argo.DTO;

public record ActivityDTO(string Id, string ProjectId, string WorkItemId, string Title, string Owner, string Status, DateOnly DueDate, string Notes);

public record ActivityCreateDTO(string ProjectId, string WorkItemId, string Title, string Owner, string Status, DateOnly DueDate, string Notes);
