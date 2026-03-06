using MeetingManagement.Common;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingManagement.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IDepartmentService _departmentService;

    public UserController(IUserService userService, ICompanyService companyService, IDepartmentService departmentService)
    {
        _userService = userService;
        _companyService = companyService;
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(PaginatedRequest request, string? companyId = null, string? departmentId = null)
    {
        var result = await _userService.Find(request, companyId, departmentId);

        var companies = await _companyService.GetAllActive();
        var departments = (await _departmentService.Find(new PaginatedRequest { PageSize = 1000 }, companyId)).Items;

        ViewBag.Companies = companies;
        ViewBag.Departments = departments;
        ViewBag.SelectedCompanyId = companyId;
        ViewBag.SelectedDepartmentId = departmentId;

        return View(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        ViewBag.Companies = await _companyService.GetAllActive();
        ViewBag.Departments = (await _departmentService.Find(new PaginatedRequest { PageSize = 1000 })).Items;
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Update(string id)
    {
        var model = await _userService.GetUpdateModelById(id);
        if (model == null)
        {
            TempData["Error"] = "Không tìm thấy người dùng!";
            return RedirectToAction("Index");
        }

        ViewBag.Companies = await _companyService.GetAllActive();
        ViewBag.Departments = (await _departmentService.Find(new PaginatedRequest { PageSize = 1000 }, model.CompanyId)).Items;
        
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserCreateModel model)
    {
        if (ModelState.IsValid)
        {
            await _userService.CreateUser(model);
            return RedirectToAction("Index");
        }
        ViewBag.Companies = await _companyService.GetAllActive();
        ViewBag.Departments = (await _departmentService.Find(new PaginatedRequest { PageSize = 1000 }, model.CompanyId)).Items;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(UserUpdateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Companies = await _companyService.GetAllActive();
            ViewBag.Departments = (await _departmentService.Find(new PaginatedRequest { PageSize = 1000 }, model.CompanyId)).Items;
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
            ViewBag.Companies = await _companyService.GetAllActive();
            ViewBag.Departments = (await _departmentService.Find(new PaginatedRequest { PageSize = 1000 }, model.CompanyId)).Items;
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
