using Argo.Domain.Common;
using Argo.Domain.Enums;
using Argo.Domain.Events;
using Argo.Domain.ValueObjects;
using FluentResults;

namespace Argo.Domain.Entities;

public class Project : AggregateRoot<ProjectId>
{
    private readonly List<WorkItem> _workItems = [];
    private readonly List<RaidItem> _raidItems = [];

    private Project(ProjectId id)
    {
        Id = id;
    }

    public string Name { get; internal set; } = string.Empty;
    public UserId? OwnerId { get; internal set; }
    public ProjectStatus Status { get; internal set; } = ProjectStatus.Waiting;
    public ProjectHealth Health { get; internal set; } = ProjectHealth.NotAssessed;
    public ProjectPriority Priority { get; internal set; } = ProjectPriority.Medium;
    public string Objective { get; internal set; } = string.Empty;
    public string NextMilestone { get; internal set; } = string.Empty;
    public DateOnly TargetDate { get; internal set; }
    public string SourceRequestId { get; internal set; } = string.Empty;
    public DateTime SubmittedAt { get; internal set; }
    public string? IntakeDetails { get; internal set; }


    public User? Owner { get; set; }
    public IReadOnlyCollection<WorkItem> WorkItems => _workItems;
    public IReadOnlyCollection<RaidItem> RaidItems => _raidItems;

    public static Result<Project> Create(ProjectId id, string name, UserId? ownerId, ProjectStatus status, ProjectHealth health, ProjectPriority priority, string objective, string nextMilestone, DateOnly targetDate,
                                            string sourceRequestId, DateTime submittedAt, string? intakeDetails = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail<Project>(DomainErrors.Required(nameof(Name)));

        if (string.IsNullOrWhiteSpace(objective))
            return Result.Fail<Project>(DomainErrors.Required(nameof(Objective)));

        if (string.IsNullOrWhiteSpace(nextMilestone))
            return Result.Fail<Project>(DomainErrors.Required(nameof(NextMilestone)));

        var project = new Project(id)
        {
            Name = name,
            OwnerId = ownerId,
            Status = status,
            Health = health,
            Priority = priority,
            Objective = objective,
            NextMilestone = nextMilestone,
            TargetDate = targetDate,
            SourceRequestId = sourceRequestId ?? string.Empty,
            SubmittedAt = submittedAt,
            IntakeDetails = intakeDetails
        };

        if (ownerId is not null)
            project.RaiseDomainEvent(new ProjectManagerChanged(id, null, ownerId));

        return Result.Ok(project);
    }

    public Result UpdateDetails(string name, UserId? ownerId, ProjectStatus status, ProjectHealth health, ProjectPriority priority, string objective, string nextMilestone, DateOnly targetDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail(DomainErrors.Required(nameof(Name)));

        if (string.IsNullOrWhiteSpace(objective))
            return Result.Fail(DomainErrors.Required(nameof(Objective)));

        if (string.IsNullOrWhiteSpace(nextMilestone))
            return Result.Fail(DomainErrors.Required(nameof(NextMilestone)));

        var previousOwnerId = OwnerId;

        Name = name;
        OwnerId = ownerId;
        Status = status;
        Health = health;
        Priority = priority;
        Objective = objective;
        NextMilestone = nextMilestone;
        TargetDate = targetDate;

        if (previousOwnerId != ownerId)
            RaiseDomainEvent(new ProjectManagerChanged(Id, previousOwnerId, ownerId));

        return Result.Ok();
    }

    internal void AddWorkItem(WorkItem workItem) => _workItems.Add(workItem);

    internal void AddRaidItem(RaidItem raidItem) => _raidItems.Add(raidItem);
}
