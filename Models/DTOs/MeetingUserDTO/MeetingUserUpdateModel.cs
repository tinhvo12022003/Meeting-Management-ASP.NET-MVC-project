using System.ComponentModel.DataAnnotations;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class MeetingUserUpdateModel
{
    [Required]
    public string UserId {get; set;} = string.Empty;

    [Required]
    public string MeetingId {get; set;} = string.Empty;

    [EnumDataType(typeof(RoleMeeting), ErrorMessage = "Invalid role!")]
    public RoleMeeting Role {get; set;}
}