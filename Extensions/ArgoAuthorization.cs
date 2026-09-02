using Argo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore;

namespace Argo.Extensions;

/// <summary>
/// Represents the requirement that the current authenticated user must exist
/// in the Argo user directory to access protected API endpoints.
/// </summary>
public sealed class ArgoUserRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Validates that the current authenticated Windows user exists in the Argo user table.
/// </summary>
/// <param name="dbContext">The EF Core context used to look up known Argo users.</param>
/// <remarks>
/// Domain-qualified identity names (e.g. <c>DOMAIN\username</c>) are normalized to the
/// account name segment before comparison with stored <c>DomainID</c> values, matching
/// the lookup previously performed inline within <c>ArgoService.CheckAuthorized</c>.
/// </remarks>
public sealed class ArgoUserAuthorizationHandler(ArgoDbContext dbContext) : AuthorizationHandler<ArgoUserRequirement>
{
    /// <summary>
    /// Evaluates whether the current user satisfies the <see cref="ArgoUserRequirement"/>.
    /// </summary>
    /// <param name="context">The authorization context containing the current user principal.</param>
    /// <param name="requirement">The requirement being evaluated.</param>
    /// <returns>A task that completes once the requirement has been evaluated.</returns>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ArgoUserRequirement requirement)
    {
        var user = context.User?.Identity?.Name;
        if (user is null)
            return;

        var separatorIndex = user.LastIndexOf('\\');
        if (separatorIndex >= 0)
            user = user[(separatorIndex + 1)..];

        var dbUser = await dbContext.Users.Where(u => u.DomainID.ToUpper() == user.ToUpper()).FirstOrDefaultAsync();

        if (dbUser is not null)
            context.Succeed(requirement);
    }
}

/// <summary>
/// Overrides the default authorization failure handling so that policy failures
/// produce the same 401 response contract the application returned before
/// authorization was centralized into ASP.NET Core policies.
/// </summary>
/// <remarks>
/// By default, ASP.NET Core returns 403 Forbidden when an authenticated user fails
/// a policy check. Existing Argo API clients expect a 401 response with an
/// <c>["Access Denied"]</c> JSON body, matching the previous behavior of
/// <c>ResultExtension.ToResults</c> for unauthorized service results.
/// </remarks>
public sealed class ArgoAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    /// <summary>
    /// Handles the outcome of authorization for the current request.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="policy">The authorization policy that was evaluated.</param>
    /// <param name="authorizeResult">The result of the policy evaluation.</param>
    /// <returns>A task representing the asynchronous middleware operation.</returns>
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new[] { "Access Denied" });
            return;
        }

        await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
