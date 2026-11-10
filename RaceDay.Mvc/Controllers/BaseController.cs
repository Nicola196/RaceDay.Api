using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Mvc.Controllers;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 1. Check if the user is logged in by looking for the RoleName
        var role = context.HttpContext.Session.GetString("RoleName");

        // 2. Get the current controller name
        var controllerName = context.RouteData.Values["controller"]?.ToString();

        // 3. If no role is found and they aren't trying to access Account, kick them to login
        if (string.IsNullOrEmpty(role) && controllerName != "Account")
        {
            context.Result = RedirectToAction("Login", "Account");
            return;
        }

        base.OnActionExecuting(context);
    }
}