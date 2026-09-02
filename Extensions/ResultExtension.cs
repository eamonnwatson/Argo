using FluentResults;

namespace Argo.Extensions;

/// <summary>
/// Provides adapters between <see cref="Result"/> values and ASP.NET Core minimal API results.
/// </summary>
public static class ResultExtension
{
    /// <summary>
    /// Converts a typed service result into an HTTP response.
    /// </summary>
    /// <typeparam name="T">The payload type carried by the successful result.</typeparam>
    /// <param name="result">The service result to translate.</param>
    /// <returns>
    /// <see cref="Results.Ok(object?)"/> for success, 401 for unauthorized metadata,
    /// 404 for not-found metadata, otherwise 400.
    /// </returns>
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

    /// <summary>
    /// Converts an untyped service result into an HTTP response.
    /// </summary>
    /// <param name="result">The service result to translate.</param>
    /// <returns>
    /// <see cref="Results.NoContent()"/> for success, 401 for unauthorized metadata,
    /// 404 for not-found metadata, otherwise 400.
    /// </returns>
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

    /// <summary>
    /// Awaits a typed result task and converts it to an HTTP response.
    /// </summary>
    /// <typeparam name="T">The payload type carried by the successful result.</typeparam>
    /// <param name="resultTask">The asynchronous service operation.</param>
    /// <returns>The translated HTTP result.</returns>
    public static async Task<IResult> ToResultsAsync<T>(this Task<Result<T>> resultTask)
    {
        var result = await resultTask;
        return result.ToResults();
    }

    /// <summary>
    /// Awaits an untyped result task and converts it to an HTTP response.
    /// </summary>
    /// <param name="resultTask">The asynchronous service operation.</param>
    /// <returns>The translated HTTP result.</returns>
    public static async Task<IResult> ToResultsAsync(this Task<Result> resultTask)
    {
        var result = await resultTask;
        return result.ToResults();
    }

    /// <summary>
    /// Awaits a typed result and maps its successful payload to a new output type.
    /// </summary>
    /// <typeparam name="T">The source payload type.</typeparam>
    /// <typeparam name="TOut">The mapped payload type.</typeparam>
    /// <param name="resultTask">The asynchronous source result operation.</param>
    /// <param name="mapper">The mapping function applied to successful payloads.</param>
    /// <returns>A mapped result preserving original success/failure state and errors.</returns>
    public static async Task<Result<TOut>> MapAsync<T, TOut>(this Task<Result<T>> resultTask, Func<T, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }
}
