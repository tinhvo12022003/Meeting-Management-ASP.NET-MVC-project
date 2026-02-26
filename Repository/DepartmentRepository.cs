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


}