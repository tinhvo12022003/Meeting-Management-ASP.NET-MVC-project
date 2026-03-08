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
    private const string ALL_ACTIVE_ROOMS_CACHE_KEY = "AllActiveMeetingRooms";

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
        if (isExisted != null)
        {
            throw new Exception(MessageConstant.EXISTED);
        }
        var room = new MeetingRoomModel
        {
            Name = model.Name,
            Capacity = model.Capacity,
            CompanyId = model.CompanyId,
            Location = model.Location,
            RowStatus = RowStatus.ACTIVE,
            CreateAt = DateTime.Now,
            CreateBy = _helper.GetCurrentUser()
        };

        await _unitOfWork.MeetingRooms.Add(room);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_ROOMS_CACHE_KEY);
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
        room.Location = model.Location;
        room.CompanyId = model.CompanyId;
        room.UpdateAt = DateTime.Now;
        room.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.MeetingRooms.Update(room);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_ROOMS_CACHE_KEY);
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

        var hasActiveMeetings = await _unitOfWork.Meetings.HasActiveMeetings(RoomId);
        if (hasActiveMeetings)
        {
            throw new Exception("Không thể xóa phòng họp này vì hiện đang có các cuộc họp được lên lịch. Vui lòng chuyển hoặc hủy các cuộc họp này trước khi xóa phòng.");
        }

        room.RowStatus = RowStatus.INACTIVE;
        room.UpdateAt = DateTime.Now;
        room.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.MeetingRooms.Update(room);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_ROOMS_CACHE_KEY);
    }
    public async Task<PaginatedResponse<MeetingRoomViewModel>> Find (PaginatedRequest request)
    {
        var paginatedResult = await _unitOfWork.MeetingRooms.GetPaginated(
            request,
            baseFilter: x => x.RowStatus == RowStatus.ACTIVE,
            searchFields: "Name,Location",
            includes: new[] { "Company" }
        );

        var viewModels = paginatedResult.Items.Select(x => new MeetingRoomViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Capacity = x.Capacity,
            Location = x.Location,
            CompanyName = x.Company?.Name ?? string.Empty,
            CompanyId = x.CompanyId
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
        var cachedData = CacheHelper.Get<List<MeetingRoomViewModel>>(ALL_ACTIVE_ROOMS_CACHE_KEY);
        if (cachedData != null) return cachedData;

        var rooms = await _unitOfWork.MeetingRooms.GetAllActive();
        var result = rooms
            .Select(x => new MeetingRoomViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Location = x.Location,
                Capacity = x.Capacity
            }).ToList();

        CacheHelper.Set(ALL_ACTIVE_ROOMS_CACHE_KEY, result, 30);
        return result;
    }

    public async Task<MeetingRoomViewModel?> GetById(string id)
    {
        var x = await _unitOfWork.MeetingRooms.GetById(id);
        if (x == null || x.RowStatus == RowStatus.INACTIVE) return null;
        return new MeetingRoomViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Capacity = x.Capacity,
            Location = x.Location,
            CompanyName = x.Company?.Name ?? string.Empty,
            CompanyId = x.CompanyId
        };
    }
}