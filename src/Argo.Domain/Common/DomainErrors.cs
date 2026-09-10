using FluentResults;

namespace Argo.Domain.Common;

public static class DomainErrors
{
    public static IError Required(string fieldName) =>
        new Error($"'{fieldName}' is required.").WithMetadata("Validation", true);

    public static IError Invalid(string fieldName, string? value) =>
        new Error($"'{value}' is not a valid value for '{fieldName}'.").WithMetadata("Validation", true);

    public static IError NotFound(string message) =>
        new Error(message).WithMetadata("NotFound", true);
}
