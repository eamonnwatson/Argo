using FluentResults;
namespace Argo.Extensions;

public static class ResultExtension
{

    public static IResult ToResults<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.ValueOrDefault);

        if (result.Errors.Where(e => e.HasMetadataKey("Unauthorized")).Any())
            return Results.Json(new[] { "Access Denied" }, statusCode: StatusCodes.Status401Unauthorized);

        if (result.Errors.Where(e => e.HasMetadataKey("NotFound")).Any())
            return Results.NotFound(result.Errors[0].Message);

        return Results.BadRequest(result.Errors[0].Message);
    }

    public static IResult ToResults(this Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        if (result.Errors.Where(e => e.HasMetadataKey("Unauthorized")).Any())
            return Results.Json(new[] { "Access Denied" }, statusCode: StatusCodes.Status401Unauthorized);

        if (result.Errors.Where(e => e.HasMetadataKey("NotFound")).Any())
            return Results.NotFound(result.Errors[0].Message);

        return Results.BadRequest(result.Errors[0].Message);
    }
    public static async Task<IResult> ToResultsAsync<T>(this Task<Result<T>> resultTask)
    {
        var result = await resultTask;
        return result.ToResults();
    }

    public static async Task<IResult> ToResultsAsync(this Task<Result> resultTask)
    {
        var result = await resultTask;
        return result.ToResults();
    }

    public static async Task<Result<TOut>> MapAsync<T, TOut>(this Task<Result<T>> resultTask, Func<T, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

}
