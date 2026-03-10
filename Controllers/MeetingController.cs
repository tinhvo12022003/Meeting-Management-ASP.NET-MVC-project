using MeetingManagement.Common;
using MeetingManagement.Enum;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using MeetingManagement.Attr.Permission;
using Microsoft.AspNetCore.Authorization;

namespace MeetingManagement.Controllers;

[Authorize]
public class MeetingController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly IMeetingRoomService _meetingRoomService;
    private readonly ICompanyService _companyService;
    private readonly IDepartmentService _departmentService;
    private readonly MeetingManagement.Helper.UserHelper _userHelper;

    public MeetingController(
        IMeetingService meetingService,
        IMeetingRoomService meetingRoomService,
        ICompanyService companyService,
        IDepartmentService departmentService,
        MeetingManagement.Helper.UserHelper userHelper)
    {
        _meetingService = meetingService;
        _meetingRoomService = meetingRoomService;
        _companyService = companyService;
        _departmentService = departmentService;
        _userHelper = userHelper;
    }

    [Permission("Meeting.Index.View")]
    public async Task<IActionResult> Index(PaginatedRequest request)
    {
        string? companyId = null;
        string? departmentId = null;

        var user = await _userHelper.GetCurrentUserProfile();
        var isAdmin = await _userHelper.IsAdmin();
        var hasFullPermission = await _userHelper.HasPermission("Meeting.*") || await _userHelper.HasPermission("Meeting.Index.FullPermission");

        if (!isAdmin && !hasFullPermission)
        {
            companyId = user?.CompanyId;
            departmentId = user?.DepartmentId;
        }

        var result = await _meetingService.Find(request, companyId, departmentId);
        
        // Prepare data for filters or dropdowns
        ViewBag.Rooms = await _meetingRoomService.GetAll();
        ViewBag.Companies = await _companyService.GetAllActive();
        ViewBag.Departments = await _departmentService.GetAllActive();
        
        return View(result);
    }

    [Permission("Meeting.Create.View")]
    public IActionResult Create()
    {
        // The 'Create' page is currently handled by AJAX/JavaScript on the index view.
        // To avoid the missing view error when the link is clicked, redirect to Index.
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Permission("Meeting.Create.Insert")]
    public async Task<IActionResult> Create(MeetingCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                var errorMessages = string.Join("; ", errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errorMessages ?? "Dữ liệu không hợp lệ." });
            }
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

    [Permission("Meeting.Update.View")]
    public async Task<IActionResult> Update(string id)
    {
        try
        {
            var model = await _meetingService.GetUpdateModel(id);
            
            // Prepare data for dropdowns
            ViewBag.Rooms = await _meetingRoomService.GetAll();
            ViewBag.Companies = await _companyService.GetAllActive();
            ViewBag.Departments = await _departmentService.GetAllActive();
            
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [Permission("Meeting.Update.Edit")]
    public async Task<IActionResult> Update(MeetingUpdateModel model)
    {
        if (!ModelState.IsValid)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                var errorMessages = string.Join("; ", errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errorMessages });
            }
            return View(model);
        }

        try
        {
            await _meetingService.Update(model);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Cuộc họp đã được cập nhật!" });
            TempData["Success"] = "Cuộc họp đã được cập nhật!";
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

    [HttpPost]
    [Permission("Meeting.Reschedule.Edit")]
    public async Task<IActionResult> Reschedule([FromBody] MeetingRescheduleModel model)
    {
        try
        {
            await _meetingService.Reschedule(model.Id, model.StartAt, model.EndAt);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [Permission("Meeting.Delete.Delete")]
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
    [Permission("Meeting.Index.View")]
    public async Task<IActionResult> GetCalendarEvents(DateTime? start, DateTime? end)
    {
        try
        {
            // FullCalendar sends 'start' and 'end' parameters as ISO8601 strings.
            // If they are null, we fall back to a reasonable default range (e.g., current month).
            DateTime from = start ?? DateTime.Now.AddMonths(-1);
            DateTime to = end ?? DateTime.Now.AddMonths(1);

            string? companyId = null;
            string? departmentId = null;

            var user = await _userHelper.GetCurrentUserProfile();
            var isAdmin = await _userHelper.IsAdmin();
            var hasFullPermission = await _userHelper.HasPermission("Meeting.*") || await _userHelper.HasPermission("Meeting.Index.FullPermission");

            if (!isAdmin && !hasFullPermission)
            {
                companyId = user?.CompanyId;
                departmentId = user?.DepartmentId;
            }

            var result = await _meetingService.GetCalendarEvents(from, to, companyId, departmentId);
            
            var events = result
                .Select(m => new {
                    id = m.Id,
                    title = m.Title,
                    start = m.StartAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = m.EndAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    url = m.Url,
                    backgroundColor = m.Color ?? "#4f46e5",
                    borderColor = m.Color ?? "#4f46e5",
                    extendedProps = new {
                        room = m.RoomName ?? "N/A",
                        company = m.CompanyName ?? "N/A",
                        department = m.DepartmentName ?? "N/A",
                        description = m.Description ?? "",
                        color = m.Color ?? "#4f46e5",
                        createdBy = m.CreatedBy ?? "Hệ thống",
                        endAt = m.EndAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    }
                })
                .ToList();
            
            return Json(events);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
}
