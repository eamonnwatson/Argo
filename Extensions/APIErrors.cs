using FluentResults;

namespace Argo.Extensions;

public static class APIErrors
{
    public static IError UnauthroizedError => new Error("Unauthorized").WithMetadata("Unauthorized",true);
    public static IError NotFoundError(string message) => new Error(message).WithMetadata("NotFound", true);
}
