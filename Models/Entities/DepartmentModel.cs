using MeetingManagement.Attr.IdPrefix;
using MeetingManagement.Models.Base;

namespace MeetingManagement.Models;

// phòng ban
[IdPrefix(prefix: "DEP")]
public class DepartmentModel : BaseModel
{
    public string Id {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;

    // relationship
    public string CompanyId {get; set;} = string.Empty;
    public CompanyModel Company {get; set;} = null!;


    public ICollection<UserModel> Users {get; set;} = new List<UserModel>();
    public ICollection<MeetingModel> Meetings {get; set;} = new List<MeetingModel>();
}