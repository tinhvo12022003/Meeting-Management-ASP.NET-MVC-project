using System.ComponentModel.DataAnnotations;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class MeetingRoomUpdateModel
{
    public string Id {get; set;} = string.Empty;

    [Required]
    [StringLength(maximumLength: 255)]
    public string Name {get; set;} = string.Empty;

    public string Location {get; set;} = string.Empty;

    public int Capacity {get; set;}

    [Required]
    public string CompanyId {get; set;} = string.Empty;

    [Required]
    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}