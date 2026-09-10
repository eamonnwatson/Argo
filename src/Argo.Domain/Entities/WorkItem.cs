using Argo.Domain.Common;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using FluentResults;

namespace Argo.Domain.Entities;

public class WorkItem : Entity<WorkItemId>
{
    private readonly List<Activity> activities = [];

    private WorkItem(WorkItemId id)
    {
        Id = id;
    }

    public ProjectId ProjectId { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string Owner { get; internal set; } = string.Empty;
    public WorkItemStatus Status { get; internal set; } = WorkItemStatus.NotStarted;
    public DateOnly DueDate { get; internal set; }
    public string Dependency { get; internal set; } = string.Empty;
    public string Purpose { get; internal set; } = string.Empty;
    public string Participants { get; internal set; } = string.Empty;
    public string RequiredInputs { get; internal set; } = string.Empty;
    public string Milestone { get; internal set; } = string.Empty;
    public string DefinitionOfDone { get; internal set; } = string.Empty;

    public IReadOnlyCollection<Activity> Activities => activities;

    public static Result<WorkItem> Create(WorkItemId id, ProjectId projectId, string title, string owner, WorkItemStatus status, DateOnly dueDate, string dependency, string purpose, string participants, string requiredInputs,
                                            string milestone, string definitionOfDone)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail<WorkItem>(DomainErrors.Required(nameof(Title)));

        if (string.IsNullOrWhiteSpace(owner))
            return Result.Fail<WorkItem>(DomainErrors.Required(nameof(Owner)));

        var workItem = new WorkItem(id)
        {
            ProjectId = projectId,
            Title = title,
            Owner = owner,
            Status = status,
            DueDate = dueDate,
            Dependency = dependency ?? string.Empty,
            Purpose = purpose ?? string.Empty,
            Participants = participants ?? string.Empty,
            RequiredInputs = requiredInputs ?? string.Empty,
            Milestone = milestone ?? string.Empty,
            DefinitionOfDone = definitionOfDone ?? string.Empty
        };

        return Result.Ok(workItem);
    }

    public Result UpdateDetails(ProjectId projectId, string title, string owner, WorkItemStatus status, DateOnly dueDate, string dependency, string purpose, string participants, string requiredInputs,
                                    string milestone, string definitionOfDone)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail(DomainErrors.Required(nameof(Title)));

        if (string.IsNullOrWhiteSpace(owner))
            return Result.Fail(DomainErrors.Required(nameof(Owner)));

        ProjectId = projectId;
        Title = title;
        Owner = owner;
        Status = status;
        DueDate = dueDate;
        Dependency = dependency ?? string.Empty;
        Purpose = purpose ?? string.Empty;
        Participants = participants ?? string.Empty;
        RequiredInputs = requiredInputs ?? string.Empty;
        Milestone = milestone ?? string.Empty;
        DefinitionOfDone = definitionOfDone ?? string.Empty;

        return Result.Ok();
    }

    internal void AddActivity(Activity activity) => activities.Add(activity);
}
