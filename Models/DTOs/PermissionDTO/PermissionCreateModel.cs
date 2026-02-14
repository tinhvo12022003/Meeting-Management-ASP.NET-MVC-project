using System.ComponentModel.DataAnnotations;

namespace MeetingManagement.Models.DTOs;

public class PermissionCreateModel
{
    [Required]
    public string GroupId {get; set;} = string.Empty;

    [Required]
    public string Controller { get; set; } = string.Empty;

    [Required]
    public string Action { get; set; } = string.Empty;

    public bool FullPermission {get; set;}

    public bool View { get; set; }

    public bool Edit { get; set; }
    public bool Delete { get; set; }
    public bool Insert { get; set; }


    public bool EditAll { get; set; }
    public bool DeleteAll { get; set; }
    public bool InsertAll { get; set; }
}