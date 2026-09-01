namespace Argo.Models;

public class WorkItem
{
    public required string Id { get; set; }
    public required string ProjectId { get; set; }
    public required string Title { get; set; }
    public required string Owner { get; set; }
    public string Status { get; set; } = "Not Started";
    public required DateOnly DueDate { get; set; }
    public required string Dependency { get; set; }
    public required string Purpose { get; set; }
    public required string Participants { get; set; }
    public required string RequiredInputs { get; set; }
    public required string Milestone { get; set; }
    public required string DefinitionOfDone { get; set; }
    public Project? Project { get; set; }
    public ICollection<Activity> Activities { get; set; } = [];
}
