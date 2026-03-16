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


        if (!await context.User.AnyAsync())
        {
            
            var company = new CompanyModel
            {
                Name = "IDI seafood", 
                Address = "Lap Vo, Dong Thap, VietNam", 
                Phone = "0779859057",
                Email = "example123@gmail.com",
                TaxCode = "023815451",  
                RowStatus = RowStatus.ACTIVE
            };

            var department = new DepartmentModel
            {
                Name = "HCNS", 
                Company = company,
                RowStatus = RowStatus.ACTIVE
            };

            var user = new UserModel
            {
                FullName = "system", 
                Address = "IDI Corp", 
                Email = "sys123@gmail.com",
                Phone = "012345678",
                Birthday = DateOnly.FromDateTime(DateTime.UtcNow),
                Department = department, 
                Company = company, 
                Gender = Gender.MALE,
                userType = UserType.ADMIN,
                RowStatus = RowStatus.ACTIVE, 
                Username = "admin", 
                HashPassword = "AQAAAAIAAYagAAAAECR3DscyOfXBdUWF3h6AZAFjF08kw2xrlysZTL5Y2Wx29h4qay07ZTohhhUXicYR/A=="
            };

            var COM_permissions = new PermissionModel
            {
                FullPermission = true,
                Controller = "Company",
                View = true,
                Edit = true,
                Delete = true,
                Insert = true,
                EditAll = true,
                DeleteAll = true,
                InsertAll = true,
                User = user,
            };


            var DEP_permissions = new PermissionModel
            {
                FullPermission = true,
                Controller = "Department",
                View = true,
                Edit = true,
                Delete = true,
                Insert = true,
                EditAll = true,
                DeleteAll = true,
                InsertAll = true,
                User = user,
            };

            var MR_permissions = new PermissionModel
            {
                FullPermission = true,
                Controller = "MeetingRoom",
                View = true,
                Edit = true,
                Delete = true,
                Insert = true,
                EditAll = true,
                DeleteAll = true,
                InsertAll = true,
                User = user,
            };

            var Meeting_permissions = new PermissionModel
            {
                FullPermission = true,
                Controller = "Meeting",
                View = true,
                Edit = true,
                Delete = true,
                Insert = true,
                EditAll = true,
                DeleteAll = true,
                InsertAll = true,
                User = user,
            };


            var User_permissions = new PermissionModel
            {
                FullPermission = true,
                Controller = "User",
                View = true,
                Edit = true,
                Delete = true,
                Insert = true,
                EditAll = true,
                DeleteAll = true,
                InsertAll = true,
                User = user,
            };

            var Auth_permissions = new PermissionModel
            {
                FullPermission = true,
                Controller = "Auth",
                View = true,
                Edit = true,
                Delete = true,
                Insert = true,
                EditAll = true,
                DeleteAll = true,
                InsertAll = true,
                User = user,
            };


            context.Company.Add(company);
            
            context.Department.Add(department);

            context.User.Add(user);

            context.Permission.Add(COM_permissions);
            context.Permission.Add(DEP_permissions);
            context.Permission.Add(MR_permissions);
            context.Permission.Add(Meeting_permissions);
            context.Permission.Add(User_permissions);
            context.Permission.Add(Auth_permissions);

            await context.SaveChangesAsync();
        }
    }
}