namespace Argo.DTO;

public record RaidItemDTO(string Id, string ProjectId, string Type, string Description, string Owner, DateOnly DueDate);

public record RaidItemCreateDTO(string ProjectId, string Type, string Description, string Owner, DateOnly DueDate);
