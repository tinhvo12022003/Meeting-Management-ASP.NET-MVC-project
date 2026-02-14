using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class UserViewModel
{
    public string Id {get; set;} = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? Email {get; set;} = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public DateOnly? Birthday { get; set; }
    public Gender Gender;

    public string CompanyName {get; set;} = string.Empty;
    public string DepartmentName {get; set;} = string.Empty;
    
}