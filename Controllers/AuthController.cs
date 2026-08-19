using Microsoft.AspNetCore.Mvc;
using FountainBistro.Web.Services.Abstractions;

namespace FountainBistro.Web.Controllers;

public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // Если пользователь уже авторизован - редирект на главную
        if (HttpContext.Items.ContainsKey("IsAuthenticated") && (bool)HttpContext.Items["IsAuthenticated"])
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            TempData["Error"] = "Введите номер телефона";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Очищаем номер от лишних символов
            var cleanPhone = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");
            if (!cleanPhone.StartsWith("7"))
                cleanPhone = "7" + cleanPhone;

            _logger.LogInformation("Login attempt with phone: {Phone}", cleanPhone);

            // Получаем или создаем пользователя
            var userId = await _userService.GetUserIdByPhoneAsync(cleanPhone);
            if (userId == null)
            {
                userId = await _userService.CreateUserAsync(cleanPhone);
                _logger.LogInformation("Created new user: {UserId} with phone: {Phone}", userId, cleanPhone);
            }

            // Устанавливаем куку
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // В разработке false, в продакшне true
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(30)
            };
            Response.Cookies.Append("userId", userId.Value.ToString(), cookieOptions);

            _logger.LogInformation("User logged in: {UserId}", userId);
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for phone: {Phone}", phone);
            TempData["Error"] = "Ошибка при входе. Попробуйте позже.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("userId");
        _logger.LogInformation("User logged out");
        return RedirectToAction(nameof(Index));
    }
}
