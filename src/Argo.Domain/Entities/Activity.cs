using Argo.Domain.Common;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using FluentResults;

namespace Argo.Domain.Entities;

public class Activity : Entity<ActivityId>
{
    private Activity(ActivityId id)
    {
        Id = id;
    }

    public ProjectId ProjectId { get; internal set; }
    public WorkItemId WorkItemId { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string Owner { get; internal set; } = string.Empty;
    public ActivityStatus Status { get; internal set; } = ActivityStatus.NotStarted;
    public DateOnly DueDate { get; internal set; }
    public string Notes { get; internal set; } = string.Empty;

    public static Result<Activity> Create(ActivityId id, ProjectId projectId, WorkItemId workItemId, string title, string owner, ActivityStatus status, DateOnly dueDate, string notes)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail<Activity>(DomainErrors.Required(nameof(Title)));

        if (string.IsNullOrWhiteSpace(owner))
            return Result.Fail<Activity>(DomainErrors.Required(nameof(Owner)));

        var activity = new Activity(id)
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            Title = title,
            Owner = owner,
            Status = status,
            DueDate = dueDate,
            Notes = notes ?? string.Empty
        };

        return Result.Ok(activity);
    }

    public Result UpdateDetails(ProjectId projectId, WorkItemId workItemId, string title, string owner, ActivityStatus status, DateOnly dueDate, string notes)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail(DomainErrors.Required(nameof(Title)));

        if (string.IsNullOrWhiteSpace(owner))
            return Result.Fail(DomainErrors.Required(nameof(Owner)));

        ProjectId = projectId;
        WorkItemId = workItemId;
        Title = title;
        Owner = owner;
        Status = status;
        DueDate = dueDate;
        Notes = notes ?? string.Empty;

        return Result.Ok();
    }
}
