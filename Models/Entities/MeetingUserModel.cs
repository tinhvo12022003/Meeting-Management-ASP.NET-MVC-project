using MeetingManagement.Enum;
using MeetingManagement.Models;

public class MeetingUserModel
{
    public string UserId { get; set; } = string.Empty;
    public UserModel? User { get; set; }

    public RoleMeeting Role {get; set;}
    public bool IsConfirmed {get; set;}

    public string MeetingId { get; set; } = string.Empty;
    public MeetingModel? Meeting { get; set; }
}
