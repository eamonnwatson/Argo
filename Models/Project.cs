namespace Argo.Models;

public class Project
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Owner { get; init; }
    public string Status { get; set; } = "Waiting";
    public string Health { get; set; } = "Not Assessed";
    public string Priority { get; set; } = "Medium";
    public required string Objective { get; init; }
    public required string NextMilestone { get; init; } 
    public required string TargetDate { get; init; }
    public required string SourceRequestId { get; init; }
    public required DateTime SubmittedAt { get; init; }

    // Raw JSON text of the original intake request payload, if this
    // project originated from a submitted request. Kept as an opaque
    // blob because the intake form's field set can change over time.
    public string? IntakeDetails { get; set; }

}
