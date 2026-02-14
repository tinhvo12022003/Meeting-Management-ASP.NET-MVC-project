using MeetingManagement.Attr.IdPrefix;
using MeetingManagement.Models.Base;

namespace MeetingManagement.Models;

[IdPrefix(prefix: "COMP")]
public class CompanyModel : BaseModel
{
    public string Id { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;

    // relationship 
    public ICollection<DepartmentModel> Departments { get; set; } = new List<DepartmentModel>();
    public ICollection<MeetingRoomModel> Rooms { get; set; } = new List<MeetingRoomModel>();
    public ICollection<MeetingModel> Meetings { get; set; } = new List<MeetingModel>();
    public ICollection<UserModel> Users { get; set; } = new List<UserModel>();
}