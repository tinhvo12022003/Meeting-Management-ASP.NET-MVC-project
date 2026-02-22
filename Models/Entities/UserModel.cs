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
    public string Username = string.Empty;
    public string HashPassword = string.Empty;


    // relationship
    public string DepartmentId {get; set;} = string.Empty;
    public DepartmentModel? Department { get; set; }

    public string CompanyId {get; set;} = string.Empty;
    public CompanyModel? Company {get; set;}


    public ICollection<MeetingUserModel> MeetingUser { get; set; } = new List<MeetingUserModel>();
    public ICollection<PermissionModel> Permissions { get; set; } = new List<PermissionModel>();
    public ICollection<RefreshTokenModel> RefreshTokens {get; set;} = new List<RefreshTokenModel>();

}