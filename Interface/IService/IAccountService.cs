using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface IAccountService
{
    public Task Register(AccountCreateModel model);
    public Task Update(AccountUpdateModel model);
    public Task Delete(string Id);
}