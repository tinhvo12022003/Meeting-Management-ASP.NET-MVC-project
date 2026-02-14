namespace MeetingManagement.Common;

public class PaginatedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; } = "asc"; 

    public Dictionary<string, string>? ColumnFilters { get; set; }
}
