using MeetingManagement.Attr.IdPrefix;

namespace MeetingManagement.Models;

[IdPrefix(prefix: "PMS")]
public class PermissionModel
{
    public string Id { get; set; } = string.Empty;
    public string? Controller { get; set; } = string.Empty;
    public string? Action { get; set; } = string.Empty;

    public bool FullPermission {get; set;}

    public bool View { get; set; }

    public bool Edit { get; set; }
    public bool Delete { get; set; }
    public bool Insert { get; set; }


    public bool EditAll { get; set; }
    public bool DeleteAll { get; set; }
    public bool InsertAll { get; set; }

    // relationship
    public string UserId { get; set; } = string.Empty;
    public UserModel User {get; set;} = null!;
}