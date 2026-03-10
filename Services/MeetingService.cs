using MeetingManagement.Common;
using MeetingManagement.Constant;
using MeetingManagement.Enum;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Service;

public class MeetingService : IMeetingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserHelper _helper;

    public MeetingService(IUnitOfWork unitOfWork, UserHelper helper)
    {
        _unitOfWork = unitOfWork;
        _helper = helper;
    }

    public async Task Create(MeetingCreateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.Title) ||
            string.IsNullOrWhiteSpace(model.RoomId)
        )
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }

        if (model.StartAt >= model.EndAt)
        {
            throw new Exception("Thời gian kết thúc phải sau thời gian bắt đầu.");
        }

        // Verify room exists and is active
        var room = await _unitOfWork.MeetingRooms.GetById(model.RoomId);
        if (room == null || room.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception("Phòng họp không tồn tại hoặc đã bị xóa. Vui lòng chọn phòng khác.");
        }

        var isExisted = await _unitOfWork.Meetings.IsMeetingOverlap(model.StartAt, model.EndAt, model.RoomId);
        if (isExisted)
        {
            throw new Exception("Phòng họp đã có lịch trong khoảng thời gian này. Vui lòng chọn thời gian hoặc phòng khác.");
        }
        var meeting = new MeetingModel
        {
            Title = model.Title,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            Type = model.Type,
            Description = model.Description,
            Organization = model.Organization,
            Url = model.Url,
            CompanyId = model.CompanyId,
            DepartmentId = model.DepartmentId,
            RoomId = model.RoomId,
            Color = model.Color,
            RowStatus = RowStatus.ACTIVE,
            CreateAt = DateTime.Now,
            CreateBy = _helper.GetCurrentUser()
        };

        await _unitOfWork.Meetings.Add(meeting);
        await _unitOfWork.CommitAsync();
    }

    public async Task Update (MeetingUpdateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.Title) || 
            string.IsNullOrWhiteSpace(model.RoomId)
        )
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }

        if (model.StartAt >= model.EndAt)
        {
            throw new Exception("Thời gian kết thúc phải sau thời gian bắt đầu.");
        }

        var meeting = await _unitOfWork.Meetings.GetById(model.Id);
        if (meeting == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        if (meeting.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }

        // Verify room exists and is active (especially if RoomId changed)
        var room = await _unitOfWork.MeetingRooms.GetById(model.RoomId);
        if (room == null || room.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception("Phòng họp không tồn tại hoặc đã bị xóa. Vui lòng chọn phòng khác.");
        }

        // Check for overlap with other meetings
        var overlap = await _unitOfWork.Meetings.IsMeetingOverlap(model.StartAt, model.EndAt, model.RoomId, model.Id);
        if (overlap)
        {
            throw new Exception("Phòng họp đã có lịch trong khoảng thời gian này. Vui lòng chọn thời gian hoặc phòng khác.");
        }

        meeting.Title = model.Title;
        meeting.StartAt = model.StartAt;
        meeting.EndAt = model.EndAt;
        meeting.Type = model.Type;
        meeting.Description = model.Description;
        meeting.Organization = model.Organization;
        meeting.Url = model.Url;
        meeting.CompanyId = model.CompanyId;
        meeting.DepartmentId = model.DepartmentId;
        meeting.RoomId = model.RoomId;
        meeting.Color = model.Color;
        //meeting.RowStatus = model.RowStatus;
        meeting.UpdateAt = DateTime.Now;
        meeting.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Meetings.Update(meeting);
        await _unitOfWork.CommitAsync();
    }

    public async Task Reschedule(string id, DateTime startAt, DateTime endAt)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new Exception(MessageConstant.EMPTY_STRING);

        var meeting = await _unitOfWork.Meetings.GetById(id);
        if (meeting == null)
            throw new Exception(MessageConstant.NOT_EXISTED);

        if (meeting.RowStatus == RowStatus.INACTIVE)
            throw new Exception(MessageConstant.INACTIVE);

        if (startAt >= endAt)
        {
            throw new Exception("Thời gian kết thúc phải sau thời gian bắt đầu.");
        }

        // collision check (exclude self)
        var overlap = await _unitOfWork.Meetings.IsMeetingOverlap(startAt, endAt, meeting.RoomId, id);
        if (overlap)
            throw new Exception("Phòng họp đã có lịch trong khoảng thời gian này.");

        meeting.StartAt = startAt;
        meeting.EndAt = endAt;
        meeting.UpdateAt = DateTime.Now;
        meeting.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Meetings.Update(meeting);
        await _unitOfWork.CommitAsync();
    }

    public async Task Delete (string Id)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var meeting = await _unitOfWork.Meetings.GetById(Id);
        if(meeting == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        if (meeting.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }

        meeting.RowStatus = RowStatus.INACTIVE;
        meeting.UpdateAt = DateTime.Now;
        meeting.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Meetings.Update(meeting);
        await _unitOfWork.CommitAsync();
    }

    public async Task<PaginatedResponse<MeetingViewModel>> Find (PaginatedRequest request, string? companyId = null, string? departmentId = null)
    {
        System.Linq.Expressions.Expression<Func<MeetingModel, bool>> filter = x => x.RowStatus == RowStatus.ACTIVE;
        
        if (!string.IsNullOrEmpty(companyId))
        {
            var originalFilter = filter;
            filter = x => x.RowStatus == RowStatus.ACTIVE && x.CompanyId == companyId;
        }
        
        if (!string.IsNullOrEmpty(departmentId))
        {
            var currentFilter = filter;
            filter = x => x.RowStatus == RowStatus.ACTIVE && (string.IsNullOrEmpty(companyId) || x.CompanyId == companyId) && x.DepartmentId == departmentId;
        }

        var paginatedResult = await _unitOfWork.Meetings.GetPaginated(
            request,
            baseFilter: filter,
            searchFields: "Title",
            includes: new[] { "MeetingRoom", "Company", "Department" }
        );

        var viewModels = paginatedResult.Items.Select(x => new MeetingViewModel
        {
            Id = x.Id,
            Title = x.Title,
            StartAt = x.StartAt,
            EndAt = x.EndAt,
            Type = x.Type,
            Description = x.Description,
            Organization = x.Organization,
            Url = x.Url,
            Color = x.Color,
            CreatedBy = x.CreateBy,
            RoomName = x.MeetingRoom?.Name ?? string.Empty,
            CompanyName = x.Company?.Name ?? string.Empty,
            DepartmentName = x.Department?.Name ?? string.Empty
        }).ToList();
        return new PaginatedResponse<MeetingViewModel>
        {
            Items = viewModels,
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };
    }

    public async Task<MeetingUpdateModel> GetUpdateModel(string id)
    {
        var meeting = await _unitOfWork.Meetings.GetById(id);
        if (meeting == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        return new MeetingUpdateModel
        {
            Id = meeting.Id,
            Title = meeting.Title,
            StartAt = meeting.StartAt,
            EndAt = meeting.EndAt,
            Type = meeting.Type,
            Description = meeting.Description,
            Organization = meeting.Organization,
            Url = meeting.Url,
            Color = meeting.Color,
            CompanyId = meeting.CompanyId,
            DepartmentId = meeting.DepartmentId,
            RoomId = meeting.RoomId,
            RowStatus = meeting.RowStatus
        };
    }

    public async Task<List<MeetingViewModel>> GetCalendarEvents(DateTime start, DateTime end, string? companyId = null, string? departmentId = null)
    {
        var meetings = await _unitOfWork.Meetings.GetByDateRange(start, end);
        
        var query = meetings.AsQueryable();
        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(x => x.CompanyId == companyId);
        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(x => x.DepartmentId == departmentId);

        return query.Select(x => new MeetingViewModel
        {
            Id = x.Id,
            Title = x.Title,
            StartAt = x.StartAt,
            EndAt = x.EndAt,
            Type = x.Type,
            Description = x.Description,
            Organization = x.Organization,
            Url = x.Url,
            Color = x.Color,
            CreatedBy = x.CreateBy,
            RoomName = x.MeetingRoom.Name ?? string.Empty,
            CompanyName = x.Company.Name ?? string.Empty,
            DepartmentName = x.Department.Name ?? string.Empty
        }).ToList();
    }
}

