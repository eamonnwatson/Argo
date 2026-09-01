using FluentResults;
namespace Argo.Extensions;

public static class ResultExtension
{

    public static IResult ToResults<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.ValueOrDefault);

        if (result.Errors.Where(e => e.HasMetadataKey("Unauthorized")).Any())
            return Results.Unauthorized();

        if (result.Errors.Where(e => e.HasMetadataKey("NotFound")).Any())
            return Results.NotFound();

        return Results.BadRequest(result.Errors.Select(e => e.Message));
    }

}
