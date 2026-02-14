namespace MeetingManagement.Models.DTOs;

public class AccountLoginResponse
{
    public string AccessToken {get;set;} = string.Empty;
    public string RefreshToken {get;set;} = string.Empty;
    public DateTime RefreshTokenExpiresAt  {get;set;}
    public UserViewModel? User {get;set;}
}