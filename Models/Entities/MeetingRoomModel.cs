using MeetingManagement.Attr.IdPrefix;
using MeetingManagement.Enum;
using MeetingManagement.Models.Base;

namespace MeetingManagement.Models;

[IdPrefix(prefix: "ROOM")]
public class MeetingRoomModel : BaseModel
{
    public string Id {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;

    // relationship
    public ICollection<MeetingModel> Meetings = new List<MeetingModel>();

    public string CompanyId {get; set;} = string.Empty;
    public CompanyModel? Company {get; set;}
}