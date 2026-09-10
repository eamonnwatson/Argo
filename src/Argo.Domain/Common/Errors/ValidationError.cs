using FluentResults;

namespace Argo.Domain.Common.Errors;

public class ValidationError : Error
{
    public ValidationError(string message, string fieldName) : base(message)
    {
        WithMetadata("ErrorCode", "DOMAIN_VALIDATION_ERROR");
        WithMetadata("FieldName", fieldName);
    }

}
