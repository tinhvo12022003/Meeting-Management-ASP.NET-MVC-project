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
        
    }

    public async Task Delete (string RoomId)
    {
        
    }

    public async Task<PaginatedResponse<MeetingRoomViewModel>> Find (PaginatedRequest request)
    {
        return null;
    }
}