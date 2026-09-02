namespace Argo.Models;

public class Project
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string Owner { get; set; }
    public string Status { get; set; } = "Waiting";
    public string Health { get; set; } = "Not Assessed";
    public string Priority { get; set; } = "Medium";
    public required string Objective { get; set; }
    public required string NextMilestone { get; set; } 
    public required DateOnly TargetDate { get; set; }
    public required string SourceRequestId { get; init; }
    public required DateTime SubmittedAt { get; set; }

    public string? IntakeDetails { get; set; }

    public ICollection<WorkItem> WorkItems { get; set; } = [];
    public ICollection<RaidItem> RaidItems { get; set; } = [];
}
