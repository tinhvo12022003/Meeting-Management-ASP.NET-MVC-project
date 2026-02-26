using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class CompanyViewModel
{
    public string Id {get; set;} = string.Empty;
    public string? Address {get; set;} = string.Empty;
    public string? Name {get; set;} = string.Empty;
    public string Phone {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string TaxCode {get; set;} = string.Empty;
    public RowStatus rowStatus {get; set;}
}