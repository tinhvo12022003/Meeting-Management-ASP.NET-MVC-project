
namespace MeetingManagement.Common;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // DataTables compatibility
    public List<T> Data => Items;
    public int RecordsTotal => TotalRecords;
    public int RecordsFiltered => TotalRecords;
}

