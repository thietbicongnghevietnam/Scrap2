using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using ScrapSystem.Web.Service.Interface;

namespace ScrapSystem.Web
{
    public class AuthorizeApiAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var token = httpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token))
            {
                RedirectToLogin(context);
                return;
            }

            if (IsTokenExpired(token))
            {
                var apiClient = httpContext.RequestServices.GetService(typeof(IApiClientService)) as IApiClientService;

                if (apiClient != null)
                {
                    var refreshed = await apiClient.RefreshTokenAsync();
                    if (!refreshed)
                    {
                        RedirectToLogin(context);
                        return;
                    }
                }
                else
                {
                    RedirectToLogin(context);
                    return;
                }
            }

            await next();
        }

        private void RedirectToLogin(ActionExecutingContext context)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo < DateTime.UtcNow.AddMinutes(-1); 
            }
            catch
            {
                return true;
            }
        }
    }
}
