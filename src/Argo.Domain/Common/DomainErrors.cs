using Argo.Domain.Common.Errors;
using FluentResults;

namespace Argo.Domain.Common;

public static class DomainErrors
{
    public static IError Required(string fieldName) =>
        new ValidationError($"'{fieldName}' is required.", fieldName);

    public static IError Invalid(string fieldName, string? value) =>
        new ValidationError($"'{value}' is not a valid value for '{fieldName}'.", fieldName);

    public static IError NotFound(string message) =>
        new Error(message).WithMetadata("NotFound", true);
}
