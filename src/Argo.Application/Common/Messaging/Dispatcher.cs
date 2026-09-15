using Argo.Application.Common.Errors;
using FluentResults;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Argo.Application.Common.Messaging;

internal class Dispatcher(IServiceProvider services) : IDispatcher
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = services.GetService(handlerType) ?? throw new InvalidOperationException($"Handler for request type {requestType.Name} not found.");
            var method = handlerType.GetMethod(nameof(IRequestHandler<,>.HandleAsync)) ?? throw new InvalidOperationException($"HandleAsync method not found in handler type {handlerType.Name}.");
            var task = (Task<TResponse>)method.Invoke(handler, [request, cancellationToken])!;

            return await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var actualException = ex is TargetInvocationException { InnerException: { } innerException }
                ? innerException
                : ex;

            if (TryCreateFailureResponse(actualException, out TResponse failureResponse))
                return failureResponse;

            ExceptionDispatchInfo.Capture(actualException).Throw();
            throw;
        }
    }

    private static bool TryCreateFailureResponse<TResponse>(Exception exception, out TResponse response)
    {
        var responseType = typeof(TResponse);
        var error = new UnexpectedAppError(exception);

        if (responseType == typeof(Result))
        {
            response = (TResponse)(object)Result.Fail(error);
            return true;
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var failMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(method => method.Name == nameof(Result.Fail)
                    && method.IsGenericMethodDefinition
                    && method.GetParameters() is [{ ParameterType: var parameterType }]
                    && parameterType == typeof(IError));

            response = (TResponse)failMethod.MakeGenericMethod(valueType).Invoke(null, [error])!;
            return true;
        }

        response = default!;
        return false;
    }
}
