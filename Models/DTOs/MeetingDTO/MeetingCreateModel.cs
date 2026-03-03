using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class MeetingCreateModel
{
    [Required]
    [StringLength(255, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5 đến 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [NotNull]
    [DataType(DataType.DateTime)]
    public DateTime StartAt { get; set; }

    [Required]
    [NotNull]
    [DataType(DataType.DateTime)]
    public DateTime EndAt { get; set; }

    public MeetingType Type { get; set; } = MeetingType.OFFLINE;

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