namespace MeetingManagement.Models.DTOs;

public class DepartmentViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = "DEPT-UNSET";
    public string ManagerName { get; set; } = "Chưa cập nhật";
    public string Location { get; set; } = "Tòa nhà chính";
    public string CompanyId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int TotalStaff { get; set; }
}