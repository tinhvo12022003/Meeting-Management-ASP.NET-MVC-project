
namespace MeetingManagement.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Ngăn chặn trang bị nhúng vào iframe (Chống Clickjacking)
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // 2. Ngăn chặn trình duyệt tự ý đoán kiểu file (Chống MIME-sniffing)
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // 3. Kiểm soát thông tin referrer gửi đi
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // 4. Content Security Policy (CSP) cơ bản
        // Chỉ cho phép script/style từ chính domain này và các nguồn tin cậy (Google Fonts, etc.)
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
            "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' https://cdn.jsdelivr.net;");

        // 5. Ngăn chặn XSS trên các trình duyệt cũ
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        await _next(context);
    }
}
