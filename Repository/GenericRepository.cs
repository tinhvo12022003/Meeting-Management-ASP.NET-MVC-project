using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using MeetingManagement.Common;
using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    public async Task<T?> GetById(string Id)
    {
        return await _dbSet.FindAsync(Id);
    }

    public async Task<T> Add(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<T> Update(T entity)
    {
        _dbSet.Update(entity);
        return entity;
    }


    /// <summary>
    /// Generic pagination với dynamic sort + search + column filters
    /// </summary>
    /// <param name="request">Yêu cầu phân trang từ client</param>
    /// <param name="baseFilter">Filter cố định (ví dụ: chỉ lấy record active, hoặc theo tenant)</param>
    /// <param name="searchFields">Các trường muốn áp dụng SearchTerm (ví dụ: "Title,Description")</param>
    /// <param name="filterExpressionBuilder">Xây dựng Expression cho ColumnFilters (tùy entity)</param>

    public async Task<PaginatedResponse<T>> GetPaginated(
        PaginatedRequest request,
        Expression<Func<T, bool>>? baseFilter = null,
        string? searchFields = null,
        Func<Dictionary<string, string>?,
        Expression<Func<T, bool>>>? filterExpressionBuilder = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();

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
            var term = request.SearchTerm.Trim().Replace("'", "''"); // escape SQL injection cơ bản

            // Tạo điều kiện OR cho nhiều trường: Title.Contains(@0) OR Description.Contains(@0)
            var searchConditions = searchFields
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(field => $"{field.Trim()}.ToLower().Contains(@0.ToLower())")
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
            string orderByClause = $"{request.SortColumn}{direction}";

            query = query.OrderBy(orderByClause); // Dynamic LINQ magic!
        }
        else
        {
            // Default sort nếu không chỉ định (tùy entity, ví dụ CreatedAt)
            // query = query.OrderBy(e => EF.Property<DateTime>(e, "CreatedAt"));
        }

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<T>
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

}