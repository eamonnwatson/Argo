using System.Text.Json.Serialization;
using FluentResults;
using Argo.Domain.Common;

namespace Argo.Domain.ValueObjects;

[JsonConverter(typeof(SingleValueJsonConverter<ProjectId>))]
public readonly record struct ProjectId
{
    public string Value { get; }

    private ProjectId(string value) => Value = value;

    public static Result<ProjectId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<ProjectId>(Common.DomainErrors.Required(nameof(ProjectId)));

        return Result.Ok(new ProjectId(value));
    }

    public static ProjectId FromTrustedValue(string value) => new(value);

    public override string ToString() => Value;
}
