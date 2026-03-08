using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class DepartmentUpdateModel
{
    public string Id {get; set;} = string.Empty;

    [Required]
    [NotNull]
    [Length(minimumLength: 5, maximumLength: 50, ErrorMessage = "Fix length required!")]
    public string Name {get; set;} = string.Empty;

    public string Location {get; set;} = string.Empty;

    [Required]
    [NotNull]
    public string CompanyId {get; set;} = string.Empty;

    [Required]
    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}