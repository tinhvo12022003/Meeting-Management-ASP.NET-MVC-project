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

        var isExisted = await _unitOfWork.Meetings.IsMeetingOverlap(model.StartAt, model.EndAt, model.RoomId);
        if (isExisted == true)
        {
            throw new Exception(MessageConstant.EXISTED);
        }
        var meeting = new MeetingModel
        {
            Title = model.Title,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            Type = model.Type,
            MeetingStatus = model.Status,
            Description = model.Description,
            Organization = model.Organization,
            Url = model.Url,
            CompanyId = model.CompanyId,
            DepartmentId = model.DepartmentId,
            RoomId = model.RoomId,
            RowStatus = model.RowStatus,
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

        var meeting = await _unitOfWork.Meetings.GetMeeting(model.StartAt, model.EndAt, model.RoomId);
        if (meeting == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        if (meeting.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }

        meeting.Title = model.Title;
        meeting.StartAt = model.StartAt;
        meeting.EndAt = model.EndAt;
        meeting.Type = model.Type;
        meeting.MeetingStatus = model.Status;
        meeting.Description = model.Description;
        meeting.Organization = model.Organization;
        meeting.Url = model.Url;
        meeting.CompanyId = model.CompanyId;
        meeting.DepartmentId = model.DepartmentId;
        meeting.RoomId = model.RoomId;
        meeting.RowStatus = model.RowStatus;
        meeting.UpdateAt = DateTime.UtcNow;
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
        meeting.UpdateAt = DateTime.UtcNow;
        meeting.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Meetings.Update(meeting);
        await _unitOfWork.CommitAsync();
    }

    public async Task<PaginatedResponse<MeetingViewModel>> Find (PaginatedRequest request)
    {
        var paginatedResult = await _unitOfWork.Meetings.GetPaginated(
            request,
            baseFilter: x => x.RowStatus == RowStatus.ACTIVE,
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
            Status = x.MeetingStatus,
            Description = x.Description,
            Organization = x.Organization,
            Url = x.Url,
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
}

