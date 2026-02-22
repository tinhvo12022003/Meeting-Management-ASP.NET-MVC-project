using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingManagement.Controllers;

[Route("user")]
public class UserController : Controller
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
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
            
            return RedirectToAction("Index");
        }
        await _userService.CreateUser(model);
        return View(model);
    }


    public async Task<IActionResult> Update(UserUpdateModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }
        await _userService.UpdateUser(model);
        return View(model);
    } 


    public async Task<IActionResult> Delete(string Id)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }
        await _userService.DeleteUser(Id);
        return View();
    }
}