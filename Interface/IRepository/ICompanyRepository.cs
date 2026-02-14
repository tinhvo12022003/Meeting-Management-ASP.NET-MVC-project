using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;
public interface ICompanyRepository : IGenericRepository<CompanyModel>
{
    public Task<CompanyModel?> GetByName (string Name);
    public Task<bool> Existed (string Name);
}