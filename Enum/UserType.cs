using System.ComponentModel.DataAnnotations;

namespace MeetingManagement.Enum;
public enum UserType
{
    [Display(Name = "Nhân viên")]
    STAFF = 1, 
    
    [Display(Name = "Quản lý bộ phận")]
    MANAGER = 2, 
    
    [Display(Name = "Quản trị viên")]
    ADMIN = 3
}