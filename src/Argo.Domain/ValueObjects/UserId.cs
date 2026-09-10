using System.Text.Json.Serialization;
using FluentResults;
using Argo.Domain.Common;

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

    public override string ToString() => Value;
}
