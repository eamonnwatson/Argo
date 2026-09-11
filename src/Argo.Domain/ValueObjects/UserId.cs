using Argo.Domain.Common;
using FluentResults;
using System.Text.Json.Serialization;

namespace Argo.Domain.ValueObjects;

[JsonConverter(typeof(SingleValueJsonConverter<UserId>))]
public readonly record struct UserId
{
    public string Value { get; }

    private UserId(string value) => Value = value;

    public static Result<UserId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<UserId>(Common.DomainErrors.Required(nameof(UserId)));

        return Result.Ok(new UserId(value));
    }

    public static UserId FromTrustedValue(string value) => new(value);

    public bool Equals(UserId other) => StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;
}
