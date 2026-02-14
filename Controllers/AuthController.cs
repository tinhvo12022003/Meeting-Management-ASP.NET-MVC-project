using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace MeetingManagement.Controllers;

[AllowAnonymous]
[Route("auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var result = await _authService.Login(dto);

            SetAuthCookies(result.AccessToken, result.RefreshToken);

            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    // ===================== REFRESH TOKEN =====================
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var result = await _authService.LoginWithToken(refreshToken);

            SetAuthCookies(result.AccessToken, result.RefreshToken);

            return Ok();
        }
        catch (SecurityException)
        {
            return Unauthorized();
        }
    }

    // ===================== LOGOUT =====================
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refresh_token"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.Logout(refreshToken);
        }

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return RedirectToAction("Login");
    }

    // ===================== COOKIE HELPER =====================
    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, //true => https
            SameSite =  /*SameSiteMode.Strict*/ SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}
