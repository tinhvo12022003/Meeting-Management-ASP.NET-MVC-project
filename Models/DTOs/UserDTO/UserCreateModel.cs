using System.ComponentModel.DataAnnotations;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class UserCreateModel
{
    [Required]
    [StringLength(maximumLength:100)]
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
    public Gender Gender { get; set; }

    [EnumDataType(typeof(UserType), ErrorMessage = "Invalid type user!")]
    public UserType UserType { get; set; }

    public string Username {get; set;} = string.Empty;
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    public string PlainPassword {get; set;} = string.Empty;

    [Compare("PlainPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPlainPassword {get; set;} = string.Empty;
    
    public string CompanyId {get; set;} = string.Empty;

    public string DepartmentId {get; set;} = string.Empty;

}