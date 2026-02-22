using System.ComponentModel.DataAnnotations;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class UserUpdateModel
{
    
    public string Id {get; set;} = string.Empty;

    [StringLength(maximumLength:100)]
    [Required]
    public string FullName { get; set; } = string.Empty;

    [StringLength(maximumLength: 255)]
    public string? Address { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    public string? Email {get; set;} = string.Empty;

    [DataType(DataType.PhoneNumber)]
    public string? Phone { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateOnly? Birthday { get; set; }

    [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender!")]
    public Gender Gender;

    public string CompanyId {get; set;} = string.Empty;
    public string DepartmentId {get; set;} = string.Empty;

    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}