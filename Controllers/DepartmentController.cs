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

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
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


        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(DepartmentCreateModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _departmentService.Create(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Update()
    {
        return View();
    }
}