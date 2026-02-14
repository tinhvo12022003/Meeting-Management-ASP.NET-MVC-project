using System.Linq.Expressions;
using MeetingManagement.Common;

namespace MeetingManagement.Interface.IRepository;


public interface IGenericRepository<T> where T : class
{
    public Task<T?> GetById(string Id);

    public Task<T> Add(T entity);
    public Task<T> Update(T entity);

    public Task<PaginatedResponse<T>> GetPaginated(
        PaginatedRequest request,
        Expression<Func<T, bool>>? baseFilter = null,
        string? searchFields = null,
        Func<Dictionary<string, string>?,
        Expression<Func<T, bool>>>? filterExpressionBuilder = null,
        CancellationToken cancellationToken = default
    );
}