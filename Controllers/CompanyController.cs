using MeetingManagement.Common;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingManagement.Controllers;

[Authorize]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index (int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        var request = new PaginatedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _companyService.Find(request);
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create ()
    {
        return View();
    }

    public async Task<IActionResult> Create (CompanyCreateModel model)
    {
        return View(model);
    }

    public async Task<IActionResult> Update()
    {
        return View();
    }

}