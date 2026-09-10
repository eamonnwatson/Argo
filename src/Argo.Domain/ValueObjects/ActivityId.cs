using FluentResults;

namespace Argo.Domain.ValueObjects;

public readonly record struct ActivityId
{
    public string Value { get; }

    private ActivityId(string value) => Value = value;

    public static Result<ActivityId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<ActivityId>(Common.DomainErrors.Required(nameof(ActivityId)));

        return Result.Ok(new ActivityId(value));
    }

    public static ActivityId FromTrustedValue(string value) => new(value);

    public override string ToString() => Value;
}
