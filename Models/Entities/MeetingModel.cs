using MeetingManagement.Attr.IdPrefix;
using MeetingManagement.Enum;
using MeetingManagement.Models.Base;

namespace MeetingManagement.Models;

[IdPrefix(prefix: "MEET")]
public class MeetingModel : BaseModel
{
    public string Id {get; set;} = string.Empty;
    public string Title {get; set;} = string.Empty;
    public DateTime StartAt {get; set;}
    public DateTime EndAt {get; set;}
    public MeetingType Type {get; set;} = MeetingType.OFFLINE;
    public MeetingStatus Status {get; set;} = MeetingStatus.PENDING;
    public string? Description {get; set;} = string.Empty;
    public string? Organization {get; set;} = string.Empty;
    public string? Url {get; set;} = string.Empty;


    // relationship
    public ICollection<MeetingUserModel> MeetingUser {get; set;} = new List<MeetingUserModel>();

    public string CompanyId {get; set;} = string.Empty;
    public CompanyModel? Company {get; set;}

    public string DepartmentId {get; set;} = string.Empty;
    public DepartmentModel? Department {get; set;}

    public string RoomId {get; set;} = string.Empty;
    public MeetingRoomModel MeetingRoom {get; set;} = null!;
}