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
using MeetingManagement.Middleware;
using MeetingManagement.Provider;
using MeetingManagement.Repository;
using MeetingManagement.Service;
using MeetingManagement.Service.Jwt;
using MeetingManagement.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(options =>
{
    // options.Filters.Add(new AuthorizeFilter());
    // Global Antiforgery: Tự động yêu cầu token cho tất cả POST/PUT/DELETE
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});



builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; // Hỗ trợ AJAX
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddAutoMapper(
    _ => { /* config action trống nếu không cần */ },
    typeof(AutoMapperConfig).Assembly
);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);



var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtConfig>()!;

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    // Cấu hình Scheme mặc định là "Mixed" để hỗ trợ cả Cookie và JWT
    options.DefaultAuthenticateScheme = "Mixed";
    options.DefaultChallengeScheme = "Mixed";

}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/auth/login";
    options.AccessDeniedPath = "/auth/login";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                return Task.CompletedTask;
            }

            context.Token = context.Request.Cookies["access_token"];
            return Task.CompletedTask;
        }
    };

})
.AddPolicyScheme("Mixed", "Mixed", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // Nếu có header Authorization hoặc có cookie access_token thì ưu tiên JWT
        if (context.Request.Headers.ContainsKey("Authorization") || context.Request.Cookies.ContainsKey("access_token"))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }
        // Mặc định dùng Cookie để xử lý redirect login cho trình duyệt
        return CookieAuthenticationDefaults.AuthenticationScheme;
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

builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IMeetingRoomRepository, MeetingRoomRepository>();
// builder.Services.AddScoped<IMeetingUserRepository, MeetingUserRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IMeetingRoomService, MeetingRoomService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
// builder.Services.AddScoped<IMeetingUserService, MeetingUserService>();


builder.Services.AddScoped<UserHelper>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<HashingLibrary>();


// Note: Authorization already configured with a fallback policy above.

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 30; // 30 days
        ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
            "public,max-age=" + durationInSeconds;
    }
});

app.UseRouting();


app.Use(async (context, next) =>
{
    await next();

    // Xử lý chuyển hướng nếu gặp lỗi 401 hoặc 403 khi truy cập giao diện web
    if ((context.Response.StatusCode == 401 || context.Response.StatusCode == 403) &&
        context.Request.Headers["Accept"].ToString().Contains("text/html"))
    {
        var path = context.Request.Path.Value ?? "";
        if (!path.Contains("/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/auth/login");
        }
    }
});

app.UseMiddleware<MeetingManagement.Middleware.TokenRefreshMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Meeting}/{action=Index}/{id?}")
    .WithStaticAssets();

await DbSeeder.SeedAccount(app.Services);
app.Run();
