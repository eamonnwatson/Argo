namespace Argo.Models;

/// <summary>
/// Represents a planned unit of delivery work for a project.
/// </summary>
public class WorkItem
{
    /// <summary>
    /// Gets or sets the unique work item identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent project.
    /// </summary>
    public required string ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the work item title displayed in tracking views.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the owner accountable for delivering the work item.
    /// </summary>
    public required string Owner { get; set; }

    /// <summary>
    /// Gets or sets delivery status. Defaults to <c>Not Started</c>.
    /// </summary>
    public string Status { get; set; } = "Not Started";

    /// <summary>
    /// Gets or sets the planned completion date for the work item.
    /// </summary>
    public required DateOnly DueDate { get; set; }

    /// <summary>
    /// Gets or sets upstream dependency details that can block execution.
    /// </summary>
    public required string Dependency { get; set; }

    /// <summary>
    /// Gets or sets the intended purpose or outcome of the work item.
    /// </summary>
    public required string Purpose { get; set; }

    /// <summary>
    /// Gets or sets participants expected to contribute to delivery.
    /// </summary>
    public required string Participants { get; set; }

    /// <summary>
    /// Gets or sets prerequisite inputs required before work starts.
    /// </summary>
    public required string RequiredInputs { get; set; }

    /// <summary>
    /// Gets or sets the milestone this work item supports.
    /// </summary>
    public required string Milestone { get; set; }

    /// <summary>
    /// Gets or sets the acceptance criteria used to declare completion.
    /// </summary>
    public required string DefinitionOfDone { get; set; }

    /// <summary>
    /// Gets or sets the related project navigation reference.
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// Gets or sets activity entries used to break down execution steps.
    /// </summary>
    public ICollection<Activity> Activities { get; set; } = [];
}
