namespace Argo.Models;

/// <summary>
/// Represents an individual activity associated with a project work item.
/// </summary>
public class Activity
{
    /// <summary>
    /// Gets the unique activity identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the parent project.
    /// </summary>
    public required string ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent work item.
    /// </summary>
    public required string WorkItemId { get; set; }

    /// <summary>
    /// Gets or sets the short activity title shown in planning views.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the owner responsible for completing the activity.
    /// </summary>
    public required string Owner { get; set; }

    /// <summary>
    /// Gets or sets the execution status. Defaults to <c>Not Started</c> for new activities.
    /// </summary>
    public string Status { get; set; } = "Not Started";

    /// <summary>
    /// Gets or sets the planned completion date for the activity.
    /// </summary>
    public required DateOnly DueDate { get; set; }

    /// <summary>
    /// Gets or sets free-form implementation notes for the activity.
    /// </summary>
    public required string Notes { get; set; }

    /// <summary>
    /// Gets or sets the related work item navigation reference.
    /// </summary>
    public WorkItem? WorkItem { get; set; }
}
