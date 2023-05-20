using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using StudyAuthApp.WebApi.Models;
using StudyAuthApp.WebApi.Helpers;

namespace StudyAuthApp.WebApi.AuthorizeHelpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IList<Role> _roles;

        public AuthorizeAttribute( params Role[] roles)
            => _roles = roles ?? Array.Empty<Role>();
        

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

            if (allowAnonymous)
                return;

            var user = (User)context.HttpContext.Items["User"];

            if (user == null || (_roles.Any() && !_roles.Contains(user.Role)))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }      
        }
    }
}
