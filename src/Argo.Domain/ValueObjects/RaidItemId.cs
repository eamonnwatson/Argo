using FluentResults;

namespace Argo.Domain.ValueObjects;

public readonly record struct RaidItemId
{
    public string Value { get; }

    private RaidItemId(string value) => Value = value;

    public static Result<RaidItemId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<RaidItemId>(Common.DomainErrors.Required(nameof(RaidItemId)));

        return Result.Ok(new RaidItemId(value));
    }

    public static RaidItemId FromTrustedValue(string value) => new(value);

    public override string ToString() => Value;
}
