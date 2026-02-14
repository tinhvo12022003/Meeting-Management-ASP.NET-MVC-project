using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;

public interface IDepartmentRepository : IGenericRepository<DepartmentModel>
{
    public Task<DepartmentModel?> GetByName (string CompanyName, string DepartmentName);
    public Task<bool> Existed (string CompanyName, string DepartmentName);
}