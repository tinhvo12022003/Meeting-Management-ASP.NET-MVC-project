using MeetingManagement.Data.Context;
using MeetingManagement.Enum;
using MeetingManagement.Library;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAccount(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hashing = scope.ServiceProvider.GetRequiredService<HashingLibrary>();
        await context.Database.MigrateAsync();


        if (!await context.Account.AnyAsync())
        {
            
            var company = new CompanyModel
            {
                Name = "IDI seafood", 
                Address = "Lap Vo, Dong Thap, VietNam", 
                RowStatus = RowStatus.ACTIVE
            };

            var department = new DepartmentModel
            {
                Name = "HCNS", 
                Company = company,
                RowStatus = RowStatus.ACTIVE
            };

            var account = new AccountModel
            {
                Id = "00001",
                Username = "admin", 
                HashPassword = hashing.HashPassword("12345678"),
                RowStatus = RowStatus.ACTIVE
            };

            var user = new UserModel
            {
                FullName = "system", 
                Address = "IDI Corp", 
                Email = "sys123@gmail.com",
                Phone = "012345678",
                Birthday = DateOnly.FromDateTime(DateTime.UtcNow),
                Account = account,
                Department = department, 
                Company = company, 
                Gender = Gender.MALE,
                RowStatus = RowStatus.ACTIVE
            };

            var permissions = new PermissionModel
            {
                FullPermission = true,
            };

            context.Company.Add(company);
            context.Department.Add(department);
            context.Account.Add(account);

            context.User.Add(user);
            context.Permission.Add(permissions);

            await context.SaveChangesAsync();
        }
    }
}