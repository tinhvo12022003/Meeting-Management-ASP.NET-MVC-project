using MeetingManagement.Common;
using MeetingManagement.Enum;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MeetingManagement.Controllers;

public class MeetingController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly IMeetingRoomService _meetingRoomService;
    private readonly ICompanyService _companyService;
    private readonly IDepartmentService _departmentService;

    public MeetingController(
        IMeetingService meetingService,
        IMeetingRoomService meetingRoomService,
        ICompanyService companyService,
        IDepartmentService departmentService)
    {
        _meetingService = meetingService;
        _meetingRoomService = meetingRoomService;
        _companyService = companyService;
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index(PaginatedRequest request)
    {
        var result = await _meetingService.Find(request);
        
        // Prepare data for filters or dropdowns
        ViewBag.Rooms = await _meetingRoomService.GetAll();
        ViewBag.Companies = (await _companyService.Find(new PaginatedRequest { PageSize = 100 })).Items;
        ViewBag.Departments = (await _departmentService.Find(new PaginatedRequest { PageSize = 100 })).Items;
        
        return View(result);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(MeetingCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            return View(model);
        }

        try
        {
            await _meetingService.Create(model);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Cuộc họp đã được tạo thành công!" });

            TempData["Success"] = "Cuộc họp đã được tạo thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ex.Message });

            TempData["Error"] = ex.Message;
            return View(model);
        }
    }

    public async Task<IActionResult> Update(string id)
    {
        // This is a placeholder, usually we'd get the meeting by id and map to UpdateModel
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _meetingService.Delete(id);
            TempData["Success"] = "Đã xóa cuộc họp!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetCalendarEvents()
    {
        // Fetch events for FullCalendar
        var result = await _meetingService.Find(new PaginatedRequest { PageSize = 1000 });
        var events = result.Items.Select(m => new {
            id = m.Id,
            title = m.Title,
            start = m.StartAt,
            end = m.EndAt,
            url = m.Url,
            extendedProps = new {
                room = m.RoomName,
                company = m.CompanyName,
                department = m.DepartmentName,
                status = m.Status.ToString()
            }
        });
        return Json(events);
    }
}
