using MeetingManagement.Attr.IdPrefix;
using MeetingManagement.Enum;
using MeetingManagement.Models.Base;

namespace MeetingManagement.Models;

[IdPrefix(prefix: "USER")]
public class UserModel : BaseModel
{
    // primary key
    public string Id {get; set;} = null!;

    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? Email {get; set;} = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public DateOnly? Birthday { get; set; }

    public Gender Gender;


    // relationship
    public string AccountId {get; set;} = string.Empty;
    public AccountModel Account { get; set; } = null!;

    public string DepartmentId {get; set;} = string.Empty;
    public DepartmentModel? Department { get; set; }

    public string CompanyId {get; set;} = string.Empty;
    public CompanyModel? Company {get; set;}


    public ICollection<MeetingUserModel> MeetingUser { get; set; } = new List<MeetingUserModel>();
    public ICollection<PermissionModel> Permissions { get; set; } = new List<PermissionModel>();

}