using Argo.Domain.Common;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using FluentResults;

namespace Argo.Domain.Entities;

public class RaidItem : Entity<RaidItemId>
{
    private RaidItem(RaidItemId id)
    {
        Id = id;
    }

    public ProjectId ProjectId { get; internal set; }
    public RaidItemType Type { get; internal set; } = RaidItemType.Risk;
    public string Description { get; internal set; } = string.Empty;
    public string Owner { get; internal set; } = string.Empty;
    public DateOnly DueDate { get; internal set; }

    public static Result<RaidItem> Create(RaidItemId id, ProjectId projectId, RaidItemType type, string description, string owner, DateOnly dueDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Fail<RaidItem>(DomainErrors.Required(nameof(Description)));

        if (string.IsNullOrWhiteSpace(owner))
            return Result.Fail<RaidItem>(DomainErrors.Required(nameof(Owner)));

        var raidItem = new RaidItem(id)
        {
            ProjectId = projectId,
            Type = type,
            Description = description,
            Owner = owner,
            DueDate = dueDate
        };

        return Result.Ok(raidItem);
    }

    public Result UpdateDetails(ProjectId projectId, RaidItemType type, string description, string owner, DateOnly dueDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Fail(DomainErrors.Required(nameof(Description)));

        if (string.IsNullOrWhiteSpace(owner))
            return Result.Fail(DomainErrors.Required(nameof(Owner)));

        ProjectId = projectId;
        Type = type;
        Description = description;
        Owner = owner;
        DueDate = dueDate;

        return Result.Ok();
    }
}
