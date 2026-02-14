using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class MeetingCreateModel
{
    [Required]
    [Length(minimumLength: 5, maximumLength: 255, ErrorMessage = "Fix length required")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [NotNull]
    [DataType(DataType.DateTime)]
    public DateTime StartAt { get; set; }

    [Required]
    [NotNull]
    [DataType(DataType.DateTime)]
    public DateTime EndAt { get; set; }

    [Required]
    [EnumDataType(typeof(MeetingType), ErrorMessage = "Invalid meeting type!")]
    public MeetingType Type { get; set; } = MeetingType.OFFLINE;

    [Required]
    [EnumDataType(typeof(MeetingStatus), ErrorMessage = "Invalid status!")]
    public MeetingStatus Status { get; set; }

    [StringLength(maximumLength: 255)]
    public string? Description { get; set; } = string.Empty;

    [StringLength(maximumLength: 255)]
    public string? Organization { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    public string? Url {get; set;} = string.Empty;

    [Required]
    [NotNull]
    public string CompanyId { get; set; } = string.Empty;

    [Required]
    [NotNull]
    public string DepartmentId { get; set; } = string.Empty;

    [Required]
    [NotNull]
    public string RoomId { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}