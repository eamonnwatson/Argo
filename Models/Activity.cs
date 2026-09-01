namespace Argo.Models;

public class Activity
{
    public required string Id { get; init; }
    public required string ProjectId { get; init; } 
    public required string WorkItemId { get; init; }
    public required string Title { get; init; }
    public required string Owner { get; init; }
    public string Status { get; set; } = "Not Started";
    public required DateTime DueDate { get; init; }
    public required string Notes { get; init; }
}
