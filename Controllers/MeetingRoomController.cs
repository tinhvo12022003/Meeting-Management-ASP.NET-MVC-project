using MeetingManagement.Common;
using MeetingManagement.Interface.IService;
using MeetingManagement.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using MeetingManagement.Attr.Permission;
using Microsoft.AspNetCore.Authorization;

namespace MeetingManagement.Controllers;

[Authorize]
public class MeetingRoomController : Controller
{
    private readonly IMeetingRoomService _meetingRoomService;
    private readonly ICompanyService _companyService;

    public MeetingRoomController(IMeetingRoomService meetingRoomService, ICompanyService companyService)
    {
        _meetingRoomService = meetingRoomService;
        _companyService = companyService;
    }

    [Permission("MeetingRoom.Index.View")]
    public async Task<IActionResult> Index(PaginatedRequest request)
    {
        var result = await _meetingRoomService.Find(request);
        return View(result);
    }

    [Permission("MeetingRoom.Create.View")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Companies = (await _companyService.Find(new PaginatedRequest { PageSize = 100 })).Items;
        return View();
    }

    [HttpPost]
    [Permission("MeetingRoom.Create.Insert")]
    public async Task<IActionResult> Create(MeetingRoomCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Companies = (await _companyService.Find(new PaginatedRequest { PageSize = 100 })).Items;
            return View(model);
        }

        try
        {
            await _meetingRoomService.Create(model);
            TempData["Success"] = "Phòng họp đã được tạo thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Companies = (await _companyService.Find(new PaginatedRequest { PageSize = 100 })).Items;
            return View(model);
        }
    }

    [Permission("MeetingRoom.Update.View")]
    public async Task<IActionResult> Update(string id)
    {
        var room = await _meetingRoomService.GetById(id);
        if (room == null) return NotFound();

        var updateModel = new MeetingRoomUpdateModel
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            CompanyId = room.CompanyId,
            RowStatus = MeetingManagement.Enum.RowStatus.ACTIVE // Defaulting or fetching
        };

        ViewBag.Companies = (await _companyService.Find(new PaginatedRequest { PageSize = 100 })).Items;
        return View(updateModel);
    }

    [HttpPost]
    [Permission("MeetingRoom.Update.Edit")]
    public async Task<IActionResult> Update(MeetingRoomUpdateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Companies = (await _companyService.Find(new PaginatedRequest { PageSize = 100 })).Items;
            return View(model);
        }

        try
        {
            await _meetingRoomService.Update(model);
            TempData["Success"] = "Cập nhật phòng họp thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Companies = (await _companyService.Find(new PaginatedRequest { PageSize = 100 })).Items;
            return View(model);
        }
    }

    [HttpPost]
    [Permission("MeetingRoom.Delete.Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _meetingRoomService.Delete(id);
            return Json(new { success = true, message = "Đã xóa phòng họp!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
