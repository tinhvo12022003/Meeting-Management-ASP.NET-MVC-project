namespace MeetingManagement.Models.DTOs;

public class MeetingRescheduleModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
