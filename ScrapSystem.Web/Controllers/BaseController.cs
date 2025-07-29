using Microsoft.AspNetCore.Mvc;
using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Web.Dtos;
using System.Text.Json;

namespace ScrapSystem.Web.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsUserAuthenticated => !string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken"));

        protected UserDto GetCurrentUser()
        {
            var userJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<UserDto>(userJson);
        }

        protected string GetCurrentToken()
        {
            return HttpContext.Session.GetString("JWTToken");
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }
    }
}
