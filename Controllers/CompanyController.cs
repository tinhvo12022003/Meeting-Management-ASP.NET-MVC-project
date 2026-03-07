using MeetingManagement.Common;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetingManagement.Attr.Permission;

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
    [Permission("Company.Index.View")]
    public async Task<IActionResult> Index (int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? status = null)
    {
        var request = new PaginatedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        if (!string.IsNullOrWhiteSpace(status))
        {
            request.ColumnFilters = new Dictionary<string, string>
            {
                { "status", status }
            };
        }

        var result = await _companyService.Find(request);
        return View(result);
    }

    [HttpGet]
    [Permission("Company.Create.View")]
    public async Task<IActionResult> Create ()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission("Company.Create.Insert")]
    public async Task<IActionResult> Create (CompanyCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _companyService.Create(model);
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Permission("Company.Update.View")]
    public async Task<IActionResult> Update(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction("Index");

        var model = await _companyService.GetById(id);
        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission("Company.Update.Edit")]
    public async Task<IActionResult> Update(CompanyUpdateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _companyService.Update(model);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission("Company.Delete.Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction("Index");

        await _companyService.Delete(id);
        return RedirectToAction("Index");
    }

}
