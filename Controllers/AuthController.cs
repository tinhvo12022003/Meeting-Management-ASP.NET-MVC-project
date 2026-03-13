using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using MeetingManagement.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetingManagement.Attr.Permission;

namespace MeetingManagement.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;

    public AuthController(IAuthService authService, IUserService userService, IPermissionService permissionService)
    {
        _authService = authService;
        _userService = userService;
        _permissionService = permissionService;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Meeting");
        
        return View();
    }

    [AllowAnonymous]
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

            return RedirectToAction("Index", "Meeting");
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

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        try
        {
            var result = await _authService.LoginWithToken(refreshToken);

            SetAuthCookies(result.AccessToken, result.RefreshToken);

            return Ok();
        }
        catch
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            return Unauthorized();
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refresh_token"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.Logout(refreshToken);
        }
        // Ensure cookies are removed by setting them expired with the same cookie options
        var secure = Request.IsHttps;

        Response.Cookies.Append("access_token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });

        Response.Cookies.Append("refresh_token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });

        // Also try Delete for good measure
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return RedirectToAction("Login");
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        var secure = Request.IsHttps;

        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }


    [Authorize]
    [HttpGet("me")]
    [Permission("Auth.Me.View")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Username = User.Identity?.Name
        });
    }

    [Authorize]
    [Permission("Auth.Authorized.View")]
    public async Task<IActionResult> Authorized (string? userId)
    {
        var users = (await _userService.Find(new PaginatedRequest { PageSize = 1000 })).Items;
        ViewBag.Users = users;
        ViewBag.SelectedUserId = userId;

        // Define the permission structure based on controllers in the project
        var permissionConfig = new List<dynamic>
        {
            new { Controller = "Meeting", DisplayName = "Quản lý Lịch họp", Actions = new[] { "Index", "Create", "Update", "Delete", "Reschedule" } },
            new { Controller = "User", DisplayName = "Quản lý Người dùng", Actions = new[] { "Index", "Register", "Update", "Delete", "Profile" } },
            new { Controller = "Company", DisplayName = "Quản lý Công ty", Actions = new[] { "Index", "Create", "Update", "Delete" } },
            new { Controller = "Department", DisplayName = "Quản lý Phòng ban", Actions = new[] { "Index", "Create", "Update", "Delete" } },
            new { Controller = "MeetingRoom", DisplayName = "Quản lý Phòng họp", Actions = new[] { "Index", "Create", "Update", "Delete" } },
            new { Controller = "Auth", DisplayName = "Phân quyền & Tài khoản", Actions = new[] { "Authorized", "Me" } }
        };

        ViewBag.PermissionConfig = permissionConfig;

        if (!string.IsNullOrEmpty(userId))
        {
            var userPermissions = (await _permissionService.Find(new PaginatedRequest { PageSize = 1000 })).Items
                .Where(p => p.Username == users.FirstOrDefault(u => u.Id == userId)?.Username)
                .ToList();
            ViewBag.UserPermissions = userPermissions;
        }

        return View();
    }

    [Authorize]
    [HttpPost("save-permissions")]
    [Permission("Auth.Authorized.Edit")]
    public async Task<IActionResult> SavePermissions([FromBody] PermissionCreateBulkModel model)
    {
        try
        {
            // First, delete existing permissions for this user to avoid duplicates if necessary, 
            // but AddBulkPermissions seems to handle it or we can update the service.
            // For now, let's assume AddBulkPermissions works as intended.
            await _permissionService.AddBulkPermissions(model);
            return Json(new { success = true, message = "Cập nhật quyền hạn thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

}
