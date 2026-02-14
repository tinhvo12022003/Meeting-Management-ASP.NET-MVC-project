using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

// view meetings each user
public class MeetingUserViewModel
{
    public string MeetingId {get; set;} = string.Empty;
    public string UserId {get; set;} = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string RoomName { get; set; } = string.Empty;       
    public RoleMeeting Role { get; set; }
    public bool IsConfirmed { get; set; } 
}