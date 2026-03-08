using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;

public interface IDepartmentRepository : IGenericRepository<DepartmentModel>
{
    public Task<DepartmentModel?> GetByName (string CompanyId, string DepartmentName);
    public Task<bool> Existed (string CompanyId, string DepartmentName);
    public Task<List<DepartmentModel>> GetAllActive();
}