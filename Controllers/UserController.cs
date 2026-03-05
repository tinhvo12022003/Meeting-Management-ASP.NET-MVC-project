using MeetingManagement.Common;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingManagement.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(PaginatedRequest request)
    {
        var result = await _userService.Find(request);
        return View(result);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserCreateModel model)
    {
        if (ModelState.IsValid)
        {
            await _userService.CreateUser(model);
            return RedirectToAction("Index");
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(UserUpdateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try 
        {
            await _userService.UpdateUser(model);
            TempData["Success"] = "Cập nhật người dùng thành công!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(model);
        }
    } 

    [HttpPost]
    public async Task<IActionResult> Delete(string Id)
    {
        try 
        {
            await _userService.DeleteUser(Id);
            TempData["Success"] = "Xóa người dùng thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Index");
    }
}
