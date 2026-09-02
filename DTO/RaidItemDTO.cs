namespace Argo.DTO;

/// <summary>
/// Represents a RAID entry returned by portfolio APIs.
/// </summary>
/// <param name="Id">The unique RAID item identifier.</param>
/// <param name="ProjectId">The owning project identifier.</param>
/// <param name="Type">The RAID category, such as risk or issue.</param>
/// <param name="Description">The RAID item description.</param>
/// <param name="Owner">The responsible owner.</param>
/// <param name="DueDate">The due date for action or resolution.</param>
public record RaidItemDTO(string Id, string ProjectId, string Type, string Description, string Owner, DateOnly DueDate);

/// <summary>
/// Represents the payload used to create a RAID item.
/// </summary>
/// <param name="ProjectId">The owning project identifier.</param>
/// <param name="Type">The RAID category, such as risk or issue.</param>
/// <param name="Description">The RAID item description.</param>
/// <param name="Owner">The responsible owner.</param>
/// <param name="DueDate">The due date for action or resolution.</param>
public record RaidItemCreateDTO(string ProjectId, string Type, string Description, string Owner, DateOnly DueDate);
