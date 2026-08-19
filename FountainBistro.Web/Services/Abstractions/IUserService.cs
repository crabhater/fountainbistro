namespace FountainBistro.Web.Services.Abstractions;

public interface IUserService
{
    Task<bool> UserExistsAsync(Guid userId);
    Task<Guid?> GetUserIdByPhoneAsync(string phone);
    Task<Guid> CreateUserAsync(string phone);
    Task<bool> ValidateUserAsync(Guid userId);
}
