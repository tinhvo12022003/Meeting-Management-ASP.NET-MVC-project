using MeetingManagement.Data.Context;
using MeetingManagement.Enum;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class CompanyRepository : GenericRepository<CompanyModel>, ICompanyRepository
{
    private readonly ApplicationDbContext _context;
    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CompanyModel?> GetByName(string Name)
    {
        return await _context.Company.FirstOrDefaultAsync(x => x.Name == Name && x.RowStatus == RowStatus.ACTIVE);
    }

    public async Task<bool> Existed (string Name)
    {
        return await _context.Company.AnyAsync(x => x.Name == Name && x.RowStatus == RowStatus.ACTIVE);
    }

    /// <summary>Filter trực tiếp ở DB — không load toàn bộ bảng Companies vào RAM.</summary>
    public async Task<List<CompanyModel>> GetAllActive()
    {
        return await _context.Company
            .Where(x => x.RowStatus == RowStatus.ACTIVE)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}