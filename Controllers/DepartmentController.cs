using MeetingManagement.Common;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingManagement.Controllers;

[Authorize]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly ICompanyService _companyService;

    public DepartmentController(IDepartmentService departmentService, ICompanyService companyService)
    {
        _departmentService = departmentService;
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? companyId = null)
    {
        var request = new PaginatedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortColumn = "Name",
            SortDirection = "asc"
        };

        var result = await _departmentService.Find(request, companyId);
        
        // Lấy danh sách công ty để hiển thị trong dropdown filter
        var companies = await _companyService.GetAllActive();
        ViewBag.Companies = companies;
        ViewBag.SelectedCompanyId = companyId;

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var companies = await _companyService.GetAllActive();
        ViewBag.Companies = companies;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(DepartmentCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            var companies = await _companyService.GetAllActive();
            ViewBag.Companies = companies;
            return View(model);
        }
        try
        {
            await _departmentService.Create(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var companies = await _companyService.GetAllActive();
            ViewBag.Companies = companies;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();

        var model = await _departmentService.GetById(id);
        if (model == null)
            return NotFound();

        var companies = await _companyService.GetAllActive();
        ViewBag.Companies = companies;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(DepartmentUpdateModel model)
    {
        if (!ModelState.IsValid)
        {
            var companies = await _companyService.GetAllActive();
            ViewBag.Companies = companies;
            return View(model);
        }

        try
        {
            await _departmentService.Update(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var companies = await _companyService.GetAllActive();
            ViewBag.Companies = companies;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _departmentService.Delete(id);
            TempData["Success"] = "Xóa phòng ban thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}