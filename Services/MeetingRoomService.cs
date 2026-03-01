using MeetingManagement.Common;
using MeetingManagement.Constant;
using MeetingManagement.Enum;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Service;
public class MeetingRoomService : IMeetingRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserHelper _helper;
    public MeetingRoomService(IUnitOfWork unitOfWork, UserHelper helper)
    {
        _unitOfWork = unitOfWork;
        _helper = helper;
    }

    public async Task Create (MeetingRoomCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }

        var isExisted = await _unitOfWork.MeetingRooms.GetByName(model.Name);
        if (isExisted == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }
        var room = new MeetingRoomModel
        {
            Name = model.Name,
            CompanyId = model.CompanyId,
            RowStatus = RowStatus.ACTIVE,
            CreateAt = DateTime.UtcNow,
            CreateBy = _helper.GetCurrentUser()
        };

        await _unitOfWork.MeetingRooms.Add(room);
        await _unitOfWork.CommitAsync();
    }

    public async Task Update (MeetingRoomUpdateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.Name) ||
            model?.Capacity == null 
        )
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var room = await _unitOfWork.MeetingRooms.GetById(model.Id);
        if (room == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }
        if (room.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }
        room.Name = model.Name;
        room.Capacity = model.Capacity;
        room.CompanyId = model.CompanyId;
        room.UpdateAt = DateTime.UtcNow;
        room.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.MeetingRooms.Update(room);
        await _unitOfWork.CommitAsync();
    }

    public async Task Delete (string RoomId)
    {
        if (string.IsNullOrWhiteSpace(RoomId))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var room = await _unitOfWork.MeetingRooms.GetById(RoomId);
        if (room == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }
        if (room.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }

        room.RowStatus = RowStatus.INACTIVE;
        room.UpdateAt = DateTime.UtcNow;
        room.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.MeetingRooms.Update(room);
        await _unitOfWork.CommitAsync();
    }
    public async Task<PaginatedResponse<MeetingRoomViewModel>> Find (PaginatedRequest request)
    {
        var paginatedResult = await _unitOfWork.MeetingRooms.GetPaginated(
            request,
            baseFilter: x => x.RowStatus == RowStatus.ACTIVE,
            searchFields: "Name",
            includes: new[] { "Company" }
        );

        var viewModels = paginatedResult.Items.Select(x => new MeetingRoomViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Capacity = x.Capacity,
            CompanyName = x.Company?.Name ?? string.Empty
        }).ToList();

        return new PaginatedResponse<MeetingRoomViewModel>
        {
            Items = viewModels,
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };
    }

    public async Task<List<MeetingRoomViewModel>> GetAll()
    {
        var rooms = await _unitOfWork.MeetingRooms.GetAll();
        return rooms.Select(x => new MeetingRoomViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Capacity = x.Capacity
        }).ToList();
    }
}