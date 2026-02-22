using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class AccountCreateModel
{
    [Length(minimumLength: 5, maximumLength: 50, ErrorMessage = "Fix length required!")]
    [Required]
    [NotNull]
    public string Username {get; set;} = string.Empty;

    [Length(minimumLength: 5, maximumLength: 50, ErrorMessage = "Fix length required!")]
    [Required]
    [NotNull]
    [DataType(DataType.Password)]
    public string PlainPassword {get; set;} = string.Empty;

    [Length(minimumLength: 5, maximumLength: 50, ErrorMessage = "Fix length required!")]
    [Required]
    [DataType(DataType.Password)]
    [Compare(otherProperty: "Password", ErrorMessage = "Password is incorrect!")]
    public string ConfirmPassword {get; set;} = string.Empty;

    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}