using System.ComponentModel.DataAnnotations;

namespace MeetingManagement.Models.DTOs;

public class PermissionCreateBulkModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Must have least 1 item")]
    public List<PermissionModel> Permissions { get; set; } = new();
}