using System.ComponentModel.DataAnnotations;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class MeetingRoomUpdateModel
{
    [Required]
    [StringLength(maximumLength: 255)]
    public string Name {get; set;} = string.Empty;

    [Required]
    public string CompanyId {get; set;} = string.Empty;

    [Required]
    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}