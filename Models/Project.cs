namespace Argo.Models;

/// <summary>
/// Represents a portfolio project created from an intake request.
/// </summary>
public class Project
{
    /// <summary>
    /// Gets the unique project identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the project name displayed in portfolio views.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the delivery owner accountable for project outcomes.
    /// </summary>
    public required string Owner { get; set; }

    /// <summary>
    /// Gets or sets the current delivery status. Defaults to <c>Waiting</c>.
    /// </summary>
    public string Status { get; set; } = "Waiting";

    /// <summary>
    /// Gets or sets health assessment for reporting. Defaults to <c>Not Assessed</c>.
    /// </summary>
    public string Health { get; set; } = "Not Assessed";

    /// <summary>
    /// Gets or sets relative project priority. Defaults to <c>Medium</c>.
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Gets or sets the business objective the project is intended to achieve.
    /// </summary>
    public required string Objective { get; set; }

    /// <summary>
    /// Gets or sets the next milestone used for near-term delivery tracking.
    /// </summary>
    public required string NextMilestone { get; set; }

    /// <summary>
    /// Gets or sets the target completion date for the project.
    /// </summary>
    public required DateOnly TargetDate { get; set; }

    /// <summary>
    /// Gets the originating intake request identifier used for traceability.
    /// </summary>
    public required string SourceRequestId { get; init; }

    /// <summary>
    /// Gets or sets when the intake request was submitted.
    /// </summary>
    public required DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets optional details captured during intake submission.
    /// </summary>
    public string? IntakeDetails { get; set; }

    /// <summary>
    /// Gets or sets work items planned for executing this project.
    /// </summary>
    public ICollection<WorkItem> WorkItems { get; set; } = [];

    /// <summary>
    /// Gets or sets RAID entries associated with this project.
    /// </summary>
    public ICollection<RaidItem> RaidItems { get; set; } = [];
}
