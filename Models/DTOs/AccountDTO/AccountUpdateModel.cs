using System.ComponentModel.DataAnnotations;
using MeetingManagement.Enum;

namespace MeetingManagement.Models.DTOs;

public class AccountUpdateModel
{
    [Length(minimumLength: 5, maximumLength: 50, ErrorMessage = "Fix length required!")]
    public string Username {get; set;} = string.Empty;
    
    [Length(minimumLength: 5, maximumLength: 50, ErrorMessage = "Fix length required!")]
    [DataType(DataType.Password)]
    public string OldPassword {get; set;} = string.Empty;


    [EnumDataType(typeof(RowStatus), ErrorMessage = "Invalid status!")]
    public RowStatus RowStatus {get; set;}
}