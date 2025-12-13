using Data.Models.Enums;

namespace Api.Services;

/// <summary>
/// Service for checking site-specific authorization and user access.
/// </summary>
public interface ISiteAuthorizationService
{
    Task<bool> HasSiteAccessAsync(int userId, int siteId);
    Task<SiteRole?> GetUserSiteRoleAsync(int userId, int siteId);
    Task<bool> HasSiteRoleAsync(int userId, int siteId, params SiteRole[] allowedRoles);
    Task<List<int>> GetUserAccessibleSiteIdsAsync(int userId);
}
