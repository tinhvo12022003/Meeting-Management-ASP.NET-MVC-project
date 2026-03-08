using MeetingManagement.Common;
using MeetingManagement.Constant;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Service;
public class MeetingUserService : IMeetingUserService
{
    private readonly IUnitOfWork _unitOfWork;
    public MeetingUserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddMember(MeetingUserCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MeetingId) || string.IsNullOrWhiteSpace(model.UserId))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }

        var existed = await _unitOfWork.MeetingUsers.GetMeetingUser(model.MeetingId, model.UserId);
        if (existed != null)
        {
            throw new Exception(MessageConstant.EXISTED);
        }

        var meetingUser = new MeetingUserModel
        {
            MeetingId = model.MeetingId,
            UserId = model.UserId,
            Role = model.Role,
        };

        await _unitOfWork.MeetingUsers.Add(meetingUser);
        await _unitOfWork.CommitAsync();
    }

    public async Task AddMembers(List<MeetingUserCreateModel> models)
    {
        if (models == null || !models.Any()) return;

        var entities = new List<MeetingUserModel>();
        foreach (var model in models)
        {
            var existed = await _unitOfWork.MeetingUsers.GetMeetingUser(model.MeetingId, model.UserId);
            if (existed == null)
            {
                entities.Add(new MeetingUserModel
                {
                    MeetingId = model.MeetingId,
                    UserId = model.UserId,
                    Role = model.Role
                });
            }
        }

        if (entities.Any())
        {
            await _unitOfWork.MeetingUsers.AddRange(entities);
            await _unitOfWork.CommitAsync();
        }
    }

    public async Task UpdateRole(MeetingUserUpdateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MeetingId) || string.IsNullOrWhiteSpace(model.UserId))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }

        var meetingUser = await _unitOfWork.MeetingUsers.GetMeetingUser(model.MeetingId, model.UserId);
        if (meetingUser == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        meetingUser.Role = model.Role;

        await _unitOfWork.MeetingUsers.Update(meetingUser);
        await _unitOfWork.CommitAsync();
    }

    public async Task RemoveMember(string meetingId, string userId)
    {
        await _unitOfWork.MeetingUsers.Delete(meetingId, userId);
        await _unitOfWork.CommitAsync();
    }

    public async Task RemoveMembers(string meetingId, List<string> userIds)
    {
        foreach (var userId in userIds)
        {
            await _unitOfWork.MeetingUsers.Delete(meetingId, userId);
        }
        await _unitOfWork.CommitAsync();
    }

    public async Task<List<MeetingUserViewModel>> GetMeetingParticipants(string meetingId)
    {
        var participants = await _unitOfWork.MeetingUsers.GetByMeetingId(meetingId);
        return participants.Select(p => new MeetingUserViewModel
        {
            MeetingId = p.MeetingId,
            UserId = p.UserId,
            FullName = p.User?.FullName ?? string.Empty,
            Role = p.Role,
            IsConfirmed = p.IsConfirmed
        }).ToList();
    }

    public async Task<PaginatedResponse<MeetingUserViewModel>> GetUserMeetings(string userId, PaginatedRequest request)
    {
        // Phân trang trực tiếp tại DB — không load toàn bộ records về RAM
        var (meetingUsers, total) = await _unitOfWork.MeetingUsers.GetPaginatedByUserId(
            userId, request.PageNumber, request.PageSize);

        var items = meetingUsers.Select(p => new MeetingUserViewModel
        {
            MeetingId = p.MeetingId,
            UserId = p.UserId,
            FullName = p.User?.FullName ?? string.Empty,
            Title = p.Meeting?.Title ?? string.Empty,
            StartAt = p.Meeting?.StartAt ?? DateTime.MinValue,
            EndAt = p.Meeting?.EndAt ?? DateTime.MinValue,
            RoomName = p.Meeting?.MeetingRoom?.Name ?? string.Empty,
            Role = p.Role,
            IsConfirmed = p.IsConfirmed
        }).ToList();

        return new PaginatedResponse<MeetingUserViewModel>
        {
            Items = items,
            TotalRecords = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}