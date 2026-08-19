using FountainBistro.Web.Services.Abstractions;

namespace FountainBistro.Web.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthMiddleware> _logger;

    public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        var userId = context.Request.Cookies["userId"];
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
        {
            if (await userService.UserExistsAsync(id))
            {
                context.Items["UserId"] = id;
                context.Items["IsAuthenticated"] = true;
                _logger.LogDebug("User authenticated: {UserId}", id);
            }
            else
            {
                context.Response.Cookies.Delete("userId");
                context.Items["IsAuthenticated"] = false;
                _logger.LogWarning("Invalid user ID in cookie: {UserId}", userId);
            }
        }
        else
        {
            context.Items["IsAuthenticated"] = false;
        }

        // Если пользователь не аутентифицирован и не на странице аутентификации
        if (!(bool)context.Items["IsAuthenticated"] && 
            !context.Request.Path.StartsWithSegments("/Auth") &&
            !context.Request.Path.StartsWithSegments("/health"))
        {
            _logger.LogDebug("Redirecting to auth page from: {Path}", context.Request.Path);
            context.Response.Redirect("/Auth/Index");
            return;
        }

        await _next(context);
    }
}
