using System.Text;
using MeetingManagement.Config;
using MeetingManagement.Config.Mapper;
using MeetingManagement.Data;
using MeetingManagement.Data.Context;
using MeetingManagement.Handler;
using MeetingManagement.Helper;
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
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});
builder.Services.AddAutoMapper(
    _ => { /* config action trống nếu không cần */ },
    typeof(AutoMapperConfig).Assembly
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);



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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Nếu Header đã có Authorization (do TokenRefreshMiddleware gán sau khi refresh), thì dùng nó.
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                return Task.CompletedTask;
            }

            context.Token = context.Request.Cookies["access_token"];
            return Task.CompletedTask;
        }
    };

});


builder.Services.AddAuthorization(options =>
{
    // Require authenticated user by default for all endpoints unless [AllowAnonymous] is present
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
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


builder.Services.AddScoped<UserHelper>();
builder.Services.AddScoped<HashingLibrary>();


// Note: Authorization already configured with a fallback policy above.

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();


app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 401 &&
        context.Request.Headers["Accept"].ToString().Contains("text/html"))
    {
        context.Response.Redirect("/auth/login");
    }
});

app.UseMiddleware<MeetingManagement.Middleware.TokenRefreshMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
await DbSeeder.SeedAccount(app.Services);
app.Run();
