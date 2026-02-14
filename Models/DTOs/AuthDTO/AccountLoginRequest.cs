using System.ComponentModel.DataAnnotations;

namespace MeetingManagement.Models.DTOs;

public class AccountLoginRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}