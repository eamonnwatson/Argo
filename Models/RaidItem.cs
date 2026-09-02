namespace Argo.Models;

/// <summary>
/// Represents a risk, assumption, issue, or dependency tracked for a project.
/// </summary>
public class RaidItem
{
    /// <summary>
    /// Gets or sets the unique RAID item identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the project that owns the RAID item.
    /// </summary>
    public required string ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the RAID category. Defaults to <c>Risk</c>.
    /// </summary>
    public required string Type { get; set; } = "Risk";

    /// <summary>
    /// Gets or sets the RAID item description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the owner responsible for mitigation or resolution.
    /// </summary>
    public required string Owner { get; set; }

    /// <summary>
    /// Gets or sets the due date for the RAID action or decision.
    /// </summary>
    public required DateOnly DueDate { get; set; }

    /// <summary>
    /// Gets or sets the related project navigation reference.
    /// </summary>
    public Project? Project { get; set; }
}
