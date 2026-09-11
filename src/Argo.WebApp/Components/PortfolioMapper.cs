using Argo.Application.DTO;
using Argo.Domain.Entities;
using Argo.Domain.Enums;

namespace Argo.WebApp.Components;

/// <summary>
/// Flattens the domain project graph into DTO collections for the Portfolio page,
/// mirroring Argo.Web.Extensions.ApiEndpoints.MapProjects.
/// </summary>
public static class PortfolioMapper
{
    public static PortfolioDTO MapProjects(IReadOnlyCollection<Project> projects)
    {
        var projectDto = projects
            .Select(p => new ProjectDTO(p.Id.Value, p.Name, p.Owner?.DisplayName ?? "Unassigned", p.Status.ToApiString(), p.Health.ToApiString(), p.Priority.ToApiString(), p.Objective, p.NextMilestone, p.TargetDate, p.SourceRequestId, p.SubmittedAt, p.IntakeDetails))
            .ToList();

        var activities = projects
            .SelectMany(p => p.WorkItems)
            .SelectMany(w => w.Activities)
            .Select(a => new ActivityDTO(a.Id.Value, a.ProjectId.Value, a.WorkItemId.Value, a.Title, a.Owner, a.Status.ToApiString(), a.DueDate, a.Notes))
            .ToList();

        var raidItems = projects
            .SelectMany(p => p.RaidItems)
            .Select(r => new RaidItemDTO(r.Id.Value, r.ProjectId.Value, r.Type.ToApiString(), r.Description, r.Owner, r.DueDate))
            .ToList();

        var workItems = projects
            .SelectMany(p => p.WorkItems)
            .Select(w => new WorkItemDTO(w!.Id.Value, w.ProjectId.Value, w.Title, w.Owner, w.Status.ToApiString(), w.DueDate, w.Dependency, w.Purpose, w.Participants, w.RequiredInputs, w.Milestone, w.DefinitionOfDone))
            .ToList();

        return new PortfolioDTO(projectDto, workItems, activities, raidItems);
    }
}
