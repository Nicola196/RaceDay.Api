using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayApi.Security;

/// <summary>
/// Requires a logged-in session. Optionally restricts access to specific roles.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public SessionAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (session.GetUserId() is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in." });
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(session.GetRole() ?? string.Empty))
        {
            context.Result = new ObjectResult(new { message = "Your role is not allowed to do this." })
            {
                StatusCode = 403
            };
        }
    }
}