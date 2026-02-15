using MeetingManagement.Attr.IdPrefix;
using MeetingManagement.Models.Base;

namespace MeetingManagement.Models;

[IdPrefix(prefix: "TOKEN")]
public class RefreshTokenModel : BaseModel
{
    public string Id {get; set;} = null!;
    public string TokenHash {get; set;} = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime LoginAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public string AccountId { get; set; } = string.Empty;
    public AccountModel Account { get; set; } = null!;
}