using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;
public interface ICompanyRepository : IGenericRepository<CompanyModel>
{
    public Task<CompanyModel?> GetByName (string Name);
    public Task<bool> Existed (string Name);
    /// <summary>Trả về danh sách công ty đang ACTIVE — filter tại DB, không load all vào RAM.</summary>
    public Task<List<CompanyModel>> GetAllActive();
}