using Data;
using Data.Models;
using Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _dbContext = context;
        _logger = logger;
    }

    public async Task<User> GetOrCreateUserFromClaimsAsync(string idpSub, string idpIssuer, string email, string? name)
    {
        var user = await _dbContext.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.IdpSub == idpSub && u.IdpIssuer == idpIssuer);

        if (user != null)
        {
            // Update user info if changed
            var hasChanges = false;
            
            if (user.Email != email)
            {
                user.Email = email;
                hasChanges = true;
            }
            
            if (user.Name != name)
            {
                user.Name = name;
                hasChanges = true;
            }
            
            if (hasChanges)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
            
            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            
            return user;
        }

        // Create new user (default to individual with User role)
        user = new User
        {
            IdpSub = idpSub,
            IdpIssuer = idpIssuer,
            Email = email,
            Name = name,
            Role = UserRole.User,
            IsIndividual = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new user from IDP: {Email} (Sub: {Sub}, Issuer: {Issuer})", 
            email, idpSub, idpIssuer);

        return user;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _dbContext.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByIdpSubAsync(string idpSub, string idpIssuer)
    {
        return await _dbContext.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.IdpSub == idpSub && u.IdpIssuer == idpIssuer);
    }
}
