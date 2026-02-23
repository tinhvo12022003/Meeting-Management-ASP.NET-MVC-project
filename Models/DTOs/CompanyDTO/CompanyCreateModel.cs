using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class CompanyCreateModel
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

    public string Phone {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string TaxCode {get; set;} = string.Empty;
}