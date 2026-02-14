using MeetingManagement.Attr.IdPrefix;
using MeetingManagement.Models.Base;

namespace MeetingManagement.Models;

[IdPrefix(prefix: "ACC")]
public class AccountModel : BaseModel
{
    public string Id {get; set;} = string.Empty;
    public string Username {get; set;} = string.Empty;
    public string HashPassword {get; set;} = string.Empty;


    // relationship
    public string UserId {get; set;} = string.Empty;
    public UserModel User {get; set;} = null!;

    public ICollection<RefreshTokenModel> RefreshTokens { get; set; } = new List<RefreshTokenModel>();
}