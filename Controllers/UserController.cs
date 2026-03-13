using MeetingManagement.Common;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetingManagement.Attr.Permission;

namespace MeetingManagement.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IDepartmentService _departmentService;
    private readonly UserHelper _userHelper;

    public UserController(IUserService userService, ICompanyService companyService, IDepartmentService departmentService, UserHelper userHelper)
    {
        _userService = userService;
        _companyService = companyService;
        _departmentService = departmentService;
        _userHelper = userHelper;
    }

    [Permission("User.Profile.View")]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = _userHelper.GetCurrentUser();
        var model = await _userService.GetUpdateModelById(userId);
        if (model == null)
        {
            TempData["Error"] = "Không tìm thấy thông tin người dùng!";
            return RedirectToAction("Index", "User");
        }

        ViewBag.Companies = await _companyService.GetAllActive();
        ViewBag.Departments = await _departmentService.GetAllActive();

        return View(model);
    }

    [Permission("User.Profile.Edit")]
    [HttpPost]
    public async Task<IActionResult> Profile(UserUpdateModel model)
    {
        var currentUserId = _userHelper.GetCurrentUser();
        
        // Security check: Ensure the user is only updating their own profile
        if (model.Id != currentUserId)
        {
            TempData["Error"] = "Bạn không có quyền cập nhật thông tin của người khác!";
            return RedirectToAction("Profile");
        }

        if (string.IsNullOrWhiteSpace(model.PlainPassword))
        {
            ModelState.Remove("PlainPassword");
            ModelState.Remove("ConfirmPlainPassword");
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Dữ liệu không hợp lệ: " + errors;

            ViewBag.Companies = await _companyService.GetAllActive();
            ViewBag.Departments = await _departmentService.GetAllActive();
            return View(model);
        }

        try
        {
            // Set some default values for empty fields if needed
            if (string.IsNullOrEmpty(model.Username))
            {
                var user = await _userService.GetUpdateModelById(currentUserId);
                if (user != null) model.Username = user.Username;
            }

            await _userService.UpdateUser(model);
            TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Companies = await _companyService.GetAllActive();
            ViewBag.Departments = await _departmentService.GetAllActive();
            return View(model);
        }
    }

    [HttpGet]
    [Permission("User.Index.View")]
    public async Task<IActionResult> Index(PaginatedRequest request, string? companyId = null, string? departmentId = null)
    {
        var result = await _userService.Find(request, companyId, departmentId);

        var companies = await _companyService.GetAllActive();
        var allDepartments = await _departmentService.GetAllActive();
        var departments = string.IsNullOrEmpty(companyId) 
            ? allDepartments 
            : allDepartments.Where(d => d.CompanyId == companyId).ToList();

        ViewBag.Companies = companies;
        ViewBag.Departments = departments;
        ViewBag.SelectedCompanyId = companyId;
        ViewBag.SelectedDepartmentId = departmentId;

        return View(result);
    }

    [Permission("User.Register.View")]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        ViewBag.Companies = await _companyService.GetAllActive();
        ViewBag.Departments = await _departmentService.GetAllActive();
        return View();
    }

    [Permission("User.Update.View")]
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
        var allDepts = await _departmentService.GetAllActive();
        ViewBag.Departments = string.IsNullOrEmpty(model.CompanyId)
            ? allDepts
            : allDepts.Where(d => d.CompanyId == model.CompanyId).ToList();
        
        return View(model);
    }

    [HttpPost]
    [Permission("User.Register.Insert")]
    public async Task<IActionResult> Register(UserCreateModel model)
    {
        if (ModelState.IsValid)
        {
            await _userService.CreateUser(model);
            return RedirectToAction("Index");
        }
        ViewBag.Companies = await _companyService.GetAllActive();
        ViewBag.Departments = await _departmentService.GetAllActive();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission("User.Update.Edit")]
    public async Task<IActionResult> Update(UserUpdateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.PlainPassword))
        {
            ModelState.Remove(nameof(model.PlainPassword));
            ModelState.Remove(nameof(model.ConfirmPlainPassword));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Companies = await _companyService.GetAllActive();
            var allDepts = await _departmentService.GetAllActive();
            ViewBag.Departments = string.IsNullOrEmpty(model.CompanyId)
                ? allDepts
                : allDepts.Where(d => d.CompanyId == model.CompanyId).ToList();
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
            var allDepts = await _departmentService.GetAllActive();
            ViewBag.Departments = string.IsNullOrEmpty(model.CompanyId)
                ? allDepts
                : allDepts.Where(d => d.CompanyId == model.CompanyId).ToList();
            return View(model);
        }
    } 

    [HttpPost]
    [Permission("User.Delete.Delete")]
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
