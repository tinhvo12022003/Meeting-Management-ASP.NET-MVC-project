using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MeetingManagement.Config;
using MeetingManagement.Interface.IService;
using Microsoft.IdentityModel.Tokens;

namespace MeetingManagement.Middleware;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtConfig _jwtConfig;

    public TokenRefreshMiddleware(RequestDelegate next, JwtConfig jwtConfig)
    {
        _next = next;
        _jwtConfig = jwtConfig;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken = context.Request.Cookies["access_token"];
        var refreshToken = context.Request.Cookies["refresh_token"];

        // Trường hợp 1: Không có access token (cookie hết hạn) nhưng có refresh token -> Thử refresh
        if (string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
        {
            await TryRefreshToken(context, refreshToken);
        }
        // Trường hợp 2: Có access token -> Kiểm tra xem còn hạn không
        else if (!string.IsNullOrEmpty(accessToken))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtConfig.Key);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtConfig.Audience,
                    ValidateLifetime = true,
                    // ClockSkew = TimeSpan.Zero // Có thể thêm nếu muốn kiểm tra chặt chẽ hơn
                };

                // Nếu token hợp lệ -> không làm gì cả
                tokenHandler.ValidateToken(accessToken, validationParameters, out _);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token hết hạn -> Thử refresh
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await TryRefreshToken(context, refreshToken);
                }
            }
            catch
            {
                // Token lỗi khác -> bỏ qua, để Auth middleware xử lý 401 sau
            }
        }

        await _next(context);
    }

    private async Task TryRefreshToken(HttpContext context, string refreshToken)
    {
        try
        {
            var authService = context.RequestServices.GetService<IAuthService>();
            if (authService == null) return;

            var result = await authService.LoginWithToken(refreshToken);

            var secure = context.Request.IsHttps;
            
            // Cập nhật Cookie mới cho response
            context.Response.Cookies.Append("access_token", result.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes)
            });

            context.Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAt
            });

            // Gán Header Authorization cho request hiện tại để Authentication Middleware phía sau dùng luôn
            if (!string.IsNullOrEmpty(result.AccessToken))
            {
                context.Request.Headers["Authorization"] = "Bearer " + result.AccessToken;
            }
        }
        catch
        {
            // Refresh thất bại (token không hợp lệ, hết hạn, v.v.) -> Không làm gì, để pipeline trả về 401
        }
    }
}
