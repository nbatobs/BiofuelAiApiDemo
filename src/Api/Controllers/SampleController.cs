using Api.Services;
using Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

/// <summary>
/// Sample controller demonstrating authentication and authorization usage.
/// Shows how to extract user information from JWT claims and implement authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SampleController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISiteAuthorizationService _siteAuthService;
    private readonly ILogger<SampleController> _logger;

    public SampleController(
        IUserService userService,
        ISiteAuthorizationService siteAuthService,
        ILogger<SampleController> logger)
    {
        _userService = userService;
        _siteAuthService = siteAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Helper method to get current user from IDP token.
    /// </summary>
    private async Task<(int? userId, string? error)> GetCurrentUserIdAsync()
    {
        var idpSub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value;
        
        if (string.IsNullOrEmpty(idpSub))
        {
            return (null, "User identifier (sub) not found in token");
        }

        var idpIssuer = User.FindFirst("iss")?.Value;
        if (string.IsNullOrEmpty(idpIssuer))
        {
            return (null, "Issuer (iss) not found in token");
        }

        var user = await _userService.GetUserByIdpSubAsync(idpSub, idpIssuer);
        if (user == null)
        {
            return (null, "User not found in database. Call /api/users/me first.");
        }

        return (user.Id, null);
    }

    /// <summary>
    /// Public endpoint - no authentication required.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public ActionResult<object> GetPublicData()
    {
        return Ok(new
        {
            message = "This is public data, accessible without authentication",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Protected endpoint - requires authentication (any authenticated user).
    /// </summary>
    [HttpGet("protected")]
    public async Task<ActionResult<object>> GetProtectedData()
    {
        var (userId, error) = await GetCurrentUserIdAsync();
        if (error != null)
        {
            return Unauthorized(new { message = error });
        }

        var user = await _userService.GetUserByIdAsync(userId!.Value);

        return Ok(new
        {
            message = "This is protected data, accessible to any authenticated user",
            user = new { user?.Email, user?.Name, UserId = userId },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Admin-only endpoint - requires Admin or SuperUser role.
    /// </summary>
    [HttpGet("admin")]
    public async Task<ActionResult<object>> GetAdminData()
    {
        var (userId, error) = await GetCurrentUserIdAsync();
        if (error != null)
        {
            return Unauthorized(new { message = error });
        }

        var user = await _userService.GetUserByIdAsync(userId!.Value);
        if (user?.Role != UserRole.Admin && user?.Role != UserRole.SuperUser)
        {
            return Forbid();
        }

        return Ok(new
        {
            message = "This is admin data, only accessible to Admin and SuperUser roles",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Manager-level endpoint - requires Manager, Admin, or SuperUser role.
    /// </summary>
    [HttpGet("manager")]
    public async Task<ActionResult<object>> GetManagerData()
    {
        var (userId, error) = await GetCurrentUserIdAsync();
        if (error != null)
        {
            return Unauthorized(new { message = error });
        }

        var user = await _userService.GetUserByIdAsync(userId!.Value);
        if (user?.Role != UserRole.Manager && user?.Role != UserRole.Admin && user?.Role != UserRole.SuperUser)
        {
            return Forbid();
        }

        return Ok(new
        {
            message = "This is manager data, accessible to Manager, Admin, and SuperUser roles",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Site-specific endpoint - checks if user has access to a specific site.
    /// Demonstrates how to check site access in the controller action.
    /// </summary>
    [HttpGet("sites/{siteId}/data")]
    public async Task<ActionResult<object>> GetSiteData(int siteId)
    {
        var (userId, error) = await GetCurrentUserIdAsync();
        if (error != null)
        {
            return Unauthorized(new { message = error });
        }

        // Check if user has access to this site
        var hasAccess = await _siteAuthService.HasSiteAccessAsync(userId!.Value, siteId);
        if (!hasAccess)
        {
            return Forbid();
        }

        // Get user's role on this site
        var siteRole = await _siteAuthService.GetUserSiteRoleAsync(userId.Value, siteId);

        return Ok(new
        {
            message = $"User has access to site {siteId}",
            siteId,
            userId,
            userRole = siteRole?.ToString(),
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Site-specific endpoint with role requirements.
    /// Only allows Operators and SiteAdmins to access specific site data.
    /// </summary>
    [HttpPost("sites/{siteId}/upload")]
    public async Task<ActionResult<object>> UploadSiteData(int siteId, [FromBody] object data)
    {
        var (userId, error) = await GetCurrentUserIdAsync();
        if (error != null)
        {
            return Unauthorized(new { message = error });
        }

        // Check if user has required role on this site
        var hasRequiredRole = await _siteAuthService.HasSiteRoleAsync(
            userId!.Value, siteId, SiteRole.Operator, SiteRole.SiteAdmin, SiteRole.Owner);

        if (!hasRequiredRole)
        {
            return Forbid();
        }

        _logger.LogInformation("User {UserId} uploading data to site {SiteId}", userId, siteId);

        // Process upload...
        return Ok(new
        {
            message = $"Data uploaded successfully to site {siteId}",
            siteId,
            uploadedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get all sites accessible to the current user.
    /// </summary>
    [HttpGet("my-sites")]
    public async Task<ActionResult<object>> GetMySites()
    {
        var (userId, error) = await GetCurrentUserIdAsync();
        if (error != null)
        {
            return Unauthorized(new { message = error });
        }

        var accessibleSiteIds = await _siteAuthService.GetUserAccessibleSiteIdsAsync(userId!.Value);

        return Ok(new
        {
            userId,
            siteCount = accessibleSiteIds.Count,
            siteIds = accessibleSiteIds,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get current user's claims (for debugging).
    /// </summary>
    [HttpGet("claims")]
    public ActionResult<object> GetClaims()
    {
        var claims = User.Claims.Select(c => new
        {
            type = c.Type,
            value = c.Value
        });

        return Ok(new
        {
            isAuthenticated = User.Identity?.IsAuthenticated,
            authenticationType = User.Identity?.AuthenticationType,
            claims
        });
    }
}
