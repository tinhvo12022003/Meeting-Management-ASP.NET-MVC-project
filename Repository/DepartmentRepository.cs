using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using MeetingManagement.Common;
using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class DepartmentRepository : GenericRepository<DepartmentModel>, IDepartmentRepository
{
    private readonly ApplicationDbContext _context;
    public DepartmentRepository (ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<DepartmentModel?> GetByName(string CompanyId, string DepartmentName)
    {
        return await _context.Department.FirstOrDefaultAsync(x => x.Company.Id == CompanyId && x.Name == DepartmentName);
    }

    public async Task<bool> Existed (string CompanyId, string DepartmentName)
    {
        return await _context.Department.AnyAsync(x => x.Company.Id == CompanyId && x.Name == DepartmentName);
    }

    public override async Task<PaginatedResponse<DepartmentModel>> GetPaginated(
        PaginatedRequest request,
        Expression<Func<DepartmentModel, bool>>? baseFilter = null,
        string? searchFields = null,
        Func<Dictionary<string, string>?, Expression<Func<DepartmentModel, bool>>>? filterExpressionBuilder = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Department.Include(x => x.Company).Include(x => x.Users).AsQueryable();

        if (baseFilter != null)
        {
            query = query.Where(baseFilter);
        }

        if (filterExpressionBuilder != null && request.ColumnFilters != null)
        {
            var extraFilter = filterExpressionBuilder(request.ColumnFilters);
            query = query.Where(extraFilter);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm) && !string.IsNullOrWhiteSpace(searchFields))
        {
            var term = request.SearchTerm.Trim().Replace("'", "''");
            var searchConditions = searchFields
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(field => $"{field}.ToLower().Contains(@0.ToLower())")
                .ToList();

            if (searchConditions.Count > 0)
            {
                var combined = string.Join(" OR ", searchConditions);
                query = query.Where(combined, term);
            }
        }

        int totalRecords = await query.CountAsync(cancellationToken);
        
        if (!string.IsNullOrWhiteSpace(request.SortColumn))
        {
            string direction = (request.SortDirection ?? "asc").ToLowerInvariant() == "desc" ? " descending" : "";
            query = query.OrderBy($"{request.SortColumn}{direction}");
        }

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<DepartmentModel>
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}