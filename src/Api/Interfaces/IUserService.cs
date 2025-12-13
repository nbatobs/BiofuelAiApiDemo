using Data.Models;

namespace Api.Services;

public interface IUserService
{
    Task<User> GetOrCreateUserFromClaimsAsync(string idpSub, string idpIssuer, string email, string? name);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByIdpSubAsync(string idpSub, string idpIssuer);
}
