using FluentResults;

namespace Argo.Domain.ValueObjects;

public readonly record struct WorkItemId
{
    public string Value { get; }

    private WorkItemId(string value) => Value = value;

    public static Result<WorkItemId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<WorkItemId>(Common.DomainErrors.Required(nameof(WorkItemId)));

        return Result.Ok(new WorkItemId(value));
    }

    public static WorkItemId FromTrustedValue(string value) => new(value);

    public override string ToString() => Value;
}
