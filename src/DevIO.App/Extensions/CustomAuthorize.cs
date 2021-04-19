using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Security.Claims;

namespace DevIO.App.Extensions
{
    public class CustomAuthorization
    {
        public static bool ValidarClaimsUsuario(HttpContext context, string clainName, string clainValue)
        {
            //return context.User.Identity.IsAuthenticated && context.User.Claims.Any(c => c.Type == clainName && c.Value.Contains(clainValue));
            return context.User.Identity.IsAuthenticated && context.User.Claims.Any(c => c.Type == clainName && c.Value.Contains(clainValue));
        }
    }

    public class ClaimsAuthorizeAttribute : TypeFilterAttribute
    {

        public ClaimsAuthorizeAttribute(string clainName,string clainValue) : base(typeof(RequisitoClaimFilter))
        {
            Arguments = new object[] { new Claim(clainName, clainValue) };
        }
    }

    public class RequisitoClaimFilter : IAuthorizationFilter
    {
        private readonly Claim _claim;

        public RequisitoClaimFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { area = "Identity", page = "/Account/Login", ReturnUrl = context.HttpContext.Request.Path.ToString() }));
                return;
            }

            if (!CustomAuthorization.ValidarClaimsUsuario(context.HttpContext,_claim.Type,_claim.Value))
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
