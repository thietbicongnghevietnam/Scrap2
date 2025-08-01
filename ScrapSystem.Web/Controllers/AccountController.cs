using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Web.Dtos;
using System.Security.Claims;
using System;
using ScrapSystem.Web.Service;
using Serilog;
using ScrapSystem.Web.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using ScrapSystem.Web;
using ScrapSystem.Api.Application.DTOs.UserDtos;
using Microsoft.IdentityModel.Tokens;
using ScrapSystem.Api.Application.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

public class AccountController : Controller
{
    private readonly IApiClientService _apiClient;

    public AccountController(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (IsUserLoggedIn())
        {
            return RedirectToAction("Index", "Scrap");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _apiClient.LoginAsync(model);
            //rao doan nay de bat cookie user truyne len
            //if (model.RememberMe)
            //{
            //    await CreateAuthenticationCookie(response.Item.User);
            //}
            //await CreateAuthenticationCookie(response.Item.User);

            if (!response.IsSuccess)
            {
                Log.Information("Invalid username or password", model.UserId);
                ModelState.AddModelError(string.Empty, response.Message ?? "Đăng nhập thất bại.");
                return View();
            }

            // Chỉ tạo cookie nếu đăng nhập thành công và có dữ liệu user
            await CreateAuthenticationCookie(response.Item.User);

            Log.Information("User {Username} logged in successfully", model.UserId);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Scrap");
        }
        catch (ApiException ex)
        {
            Log.Warning("Login failed for user {Username}: {Error}", model.UserId, ex.Message);

        
                ModelState.AddModelError("", "Login failed. Please try again.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error during login for user {Username}", model.UserId);
            ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Gọi API logout
            var response = await _apiClient.LogoutAsync();
            Log.Information("User logged out successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during logout");
        }

        HttpContext.Session.Clear();
        
        return RedirectToAction("Login", "Account");
    }


    public IActionResult AccessDenied()
    {
        return View();
    }

    private bool IsUserLoggedIn()
    {
        var token = HttpContext.Session.GetString("JWTToken");
        return !string.IsNullOrEmpty(token);
    }

    private async Task CreateAuthenticationCookie(UserDto user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserId),
        };


        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
    

    [HttpGet]
    [AuthorizeApi] 
    public IActionResult GetToken()
    {
        try
        {
            var token = HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "No token found" });
            }

            return Ok(new { token = token });
        }
        catch
        {
            return StatusCode(500, new { message = "Error retrieving token" });
        }
    }
}