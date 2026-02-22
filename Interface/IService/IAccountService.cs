using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface IAccountService
{
    public Task Register(AccountCreateModel model);
    public Task ChangePassword(AccountUpdateModel model, string NewPassword, string ConfirmPassword);
    public Task Delete(string Id);
}