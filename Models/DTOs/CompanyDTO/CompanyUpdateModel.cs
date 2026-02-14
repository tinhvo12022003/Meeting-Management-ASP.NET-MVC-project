using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class CompanyUpdateModel
{
    [Required]
    [NotNull]
    public string Address {get; set;} = string.Empty;

    [Required]
    [NotNull]
    public string Name {get; set;} = string.Empty;

    [Required]
    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}