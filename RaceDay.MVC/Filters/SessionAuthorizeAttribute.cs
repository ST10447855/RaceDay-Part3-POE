
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.MVC.Filters
{
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public SessionAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            var userEmail = session.GetString("UserEmail");
            var userRole = session.GetString("UserRole");

            // Check whether the user is logged in
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                context.Result = new RedirectToActionResult(
                    "Login", "Account", null);
                return;
            }

            // Check whether the user's role is allowed
            if (_allowedRoles.Length > 0 &&
                !_allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult(
                    "Index", "Home", null);
            }
        }
    }
}
