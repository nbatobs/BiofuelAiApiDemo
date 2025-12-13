using Data;
using Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Services;

public class SiteAuthorizationService : ISiteAuthorizationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SiteAuthorizationService> _logger;

    public SiteAuthorizationService(AppDbContext context, ILogger<SiteAuthorizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasSiteAccessAsync(int userId, int siteId)
    {
        return await _context.UserSiteAccesses
            .AnyAsync(usa => usa.UserId == userId && usa.SiteId == siteId);
    }

    public async Task<SiteRole?> GetUserSiteRoleAsync(int userId, int siteId)
    {
        var access = await _context.UserSiteAccesses
            .FirstOrDefaultAsync(usa => usa.UserId == userId && usa.SiteId == siteId);

        return access?.RoleOnSite;
    }

    public async Task<bool> HasSiteRoleAsync(int userId, int siteId, params SiteRole[] allowedRoles)
    {
        // SuperUser and Admin have access to all sites
        var user = await _context.Users.FindAsync(userId);
        if (user != null && (user.Role == UserRole.SuperUser || user.Role == UserRole.Admin))
        {
            return true;
        }

        var userRole = await GetUserSiteRoleAsync(userId, siteId);
        
        if (!userRole.HasValue)
        {
            return false;
        }

        return allowedRoles.Contains(userRole.Value);
    }

    public async Task<List<int>> GetUserAccessibleSiteIdsAsync(int userId)
    {
        // SuperUser and Admin can access all sites
        var user = await _context.Users.FindAsync(userId);
        if (user != null && (user.Role == UserRole.SuperUser || user.Role == UserRole.Admin))
        {
            return await _context.Sites.Select(s => s.Id).ToListAsync();
        }

        return await _context.UserSiteAccesses
            .Where(usa => usa.UserId == userId)
            .Select(usa => usa.SiteId)
            .ToListAsync();
    }
}
