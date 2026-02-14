using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class MeetingViewModel
{
    public string Id {get; set;} = string.Empty;
    public string Title {get; set;} = string.Empty;
    public DateTime StartAt {get; set;}
    public DateTime EndAt {get; set;}
    public MeetingType Type {get; set;} = MeetingType.OFFLINE;
    public MeetingStatus Status {get; set;}
    public string? Description {get; set;} = string.Empty;
    public string? Organization {get; set;} = string.Empty;
    public string? Url {get; set;} = string.Empty;

    
    public string RoomName {get; set;} = string.Empty;
    public string CompanyName {get; set;} = string.Empty;
    public string DepartmentName {get; set;} = string.Empty;
}