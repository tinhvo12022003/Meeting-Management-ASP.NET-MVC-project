using MeetingManagement.Data.Context;
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
        return await _context.Company.FirstOrDefaultAsync(x => x.Name == Name);
    }

    public async Task<bool> Existed (string Name)
    {
        return await _context.Company.AnyAsync(x => x.Name == Name);
    }
}