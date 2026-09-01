namespace Argo.Models;

public class RaidItem
{
    public required string Id { get; set; } 
    public required string ProjectId { get; set; } 
    public required string Type { get; set; } = "Risk";
    public required string Description { get; set; } 
    public required string Owner { get; set; }
    public required DateOnly DueDate { get; set; }

    public Project? Project { get; set; }
}
