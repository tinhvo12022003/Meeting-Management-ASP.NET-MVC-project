using MeetingManagement.Enum;

namespace MeetingManagement.Models.Base;

public abstract class BaseModel
{
    public string CreateBy {get; set;} = null!;
    public string UpdateBy {get; set;} = null!;

    public DateTime CreateAt;
    public DateTime UpdateAt;
    public RowStatus rowStatus {get; set;}
}