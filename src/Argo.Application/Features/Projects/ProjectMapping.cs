using Argo.Application.Common.Mapping;
using Argo.Domain.Entities;
using Argo.Domain.Enums;

namespace Argo.Application.Features.Projects;

internal class ProjectMapping : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<Project, ProjectDto>(project => new ProjectDto(
            Id: project.Id.Value.ToString(),
            Name: project.Name,
            Owner: project.Owner?.DisplayName ?? "Unassigned",
            Status: mapper.Map<string>(project.Status),
            Health: mapper.Map<string>(project.Health),
            Priority: mapper.Map<string>(project.Priority),
            Objective: project.Objective,
            NextMilestone: project.NextMilestone,
            TargetDate: project.TargetDate,
            SourceRequestId: project.SourceRequestId,
            IntakeDetails: project.IntakeDetails ?? string.Empty
            ));

        mapper.Register<ProjectStatus, string>(status => status switch
            {
                ProjectStatus.Waiting => "Waiting",
                ProjectStatus.InProgress => "In Progress",
                ProjectStatus.Done => "Completed",
                _ => "Unknown",
            });

        mapper.Register<ProjectHealth, string>(health => health switch
            {
                ProjectHealth.NotAssessed => "Not Assessed",
                ProjectHealth.AtRisk => "At Risk",
                ProjectHealth.Blocked => "Blocked",
                ProjectHealth.Complete => "Complete",
                ProjectHealth.OnTrack => "On Track",
                _ => "Unknown",
            });

        mapper.Register<ProjectPriority, string>(priority => priority switch
            {
                ProjectPriority.Low => "Low",
                ProjectPriority.Medium => "Medium",
                ProjectPriority.High => "High",
                ProjectPriority.Critical => "Critical",
                ProjectPriority.NeedsTriage => "Needs Triage",
                _ => "Unknown",
            });

    }
}
