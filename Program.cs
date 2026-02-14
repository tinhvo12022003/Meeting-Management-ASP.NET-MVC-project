using System.Text;
using MeetingManagement.Config;
using MeetingManagement.Config.Mapper;
using MeetingManagement.Data.Context;
using MeetingManagement.Handler;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Library;
using MeetingManagement.Provider;
using MeetingManagement.Repository;
using MeetingManagement.Service;
using MeetingManagement.Service.Jwt;
using MeetingManagement.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(
    _ => { /* config action trống nếu không cần */ },
    typeof(AutoMapperConfig).Assembly
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtConfig>()!;

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, 
        ValidateAudience = true,   
        ValidateLifetime = true, 
        ValidateIssuerSigningKey = true, 
        ValidIssuer              = jwtSettings.Issuer,
        ValidAudience            = jwtSettings.Audience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
    
});


builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IMeetingRoomRepository, MeetingRoomRepository>();
builder.Services.AddScoped<IMeetingUserRepository, MeetingUserRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<HashingLibrary>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
