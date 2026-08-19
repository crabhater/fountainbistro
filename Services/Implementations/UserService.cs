using FountainBistro.Web.Services.Abstractions;
using FountainBistro.Web.Models.Entities;
using FountainBistro.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FountainBistro.Web.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _dbContext.Users.AnyAsync(u => u.Id == userId && u.IsActive);
    }

    public async Task<Guid?> GetUserIdByPhoneAsync(string phone)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Phone == phone && u.IsActive);
        return user?.Id;
    }

    public async Task<Guid> CreateUserAsync(string phone)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Phone = phone,
            Name = null,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Created user: {UserId} with phone: {Phone}", user.Id, phone);
        return user.Id;
    }

    public async Task<bool> ValidateUserAsync(Guid userId)
    {
        return await UserExistsAsync(userId);
    }
}
