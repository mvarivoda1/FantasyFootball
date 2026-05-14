using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FantasyFootball.Filters
{
    public class RequireFantasyTeamFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpUser = context.HttpContext.User;
            if (httpUser?.Identity?.IsAuthenticated != true) return;

            if (httpUser.HasClaim(c => c.Type == "FantasyTeamId")) return;

            var route = context.RouteData.Values;
            var controller = (route["controller"]?.ToString() ?? string.Empty).ToLowerInvariant();
            var action = (route["action"]?.ToString() ?? string.Empty).ToLowerInvariant();

            if (controller == "account") return;
            if (controller == "fantasyteam" && action == "build") return;

            context.Result = new RedirectToActionResult("Build", "FantasyTeam", null);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
