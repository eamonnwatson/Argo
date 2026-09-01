namespace Argo.Models;

public class Activity
{
    public required string Id { get; init; }
    public required string ProjectId { get; set; } 
    public required string WorkItemId { get; set; }
    public required string Title { get; set; }
    public required string Owner { get; set; }
    public string Status { get; set; } = "Not Started";
    public required DateOnly DueDate { get; set; }
    public required string Notes { get; set; }
    public WorkItem? WorkItem { get; set; }
}
