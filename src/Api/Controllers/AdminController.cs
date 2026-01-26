using Api.DTOs.Admin;
using Data;
using Data.Models;
using Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// Admin endpoints for system setup: Companies, Sites, Users, and Access Control.
/// These endpoints are restricted to users with Admin or SuperUser roles.
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Companies

    /// <summary>
    /// Get all companies with summary counts.
    /// </summary>
    [HttpGet("companies")]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
    {
        var companies = await _context.Companies
            .Select(c => new CompanyDto(
                c.Id,
                c.Name,
                c.CreatedAt,
                _context.Sites.Count(s => s.CompanyId == c.Id),
                _context.Users.Count(u => u.CompanyId == c.Id)
            ))
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(companies);
    }

    /// <summary>
    /// Get a specific company by ID.
    /// </summary>
    [HttpGet("companies/{companyId:int}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(int companyId)
    {
        var company = await _context.Companies
            .Where(c => c.Id == companyId)
            .Select(c => new CompanyDto(
                c.Id,
                c.Name,
                c.CreatedAt,
                _context.Sites.Count(s => s.CompanyId == c.Id),
                _context.Users.Count(u => u.CompanyId == c.Id)
            ))
            .FirstOrDefaultAsync();

        if (company == null)
            return NotFound(new { message = $"Company with ID {companyId} not found" });

        return Ok(company);
    }

    /// <summary>
    /// Create a new company.
    /// </summary>
    [HttpPost("companies")]
    public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        // Check for duplicate name
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower());

        if (existingCompany != null)
            return Conflict(new { message = $"A company with name '{request.Name}' already exists" });

        var company = new Company
        {
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created company {CompanyId}: {CompanyName}", company.Id, company.Name);

        var dto = new CompanyDto(company.Id, company.Name, company.CreatedAt, 0, 0);
        return CreatedAtAction(nameof(GetCompany), new { companyId = company.Id }, dto);
    }

    /// <summary>
    /// Update a company.
    /// </summary>
    [HttpPatch("companies/{companyId:int}")]
    public async Task<ActionResult<CompanyDto>> UpdateCompany(int companyId, [FromBody] UpdateCompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(companyId);
        if (company == null)
            return NotFound(new { message = $"Company with ID {companyId} not found" });

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            // Check for duplicate name (excluding current company)
            var duplicate = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id != companyId && c.Name.ToLower() == request.Name.ToLower());

            if (duplicate != null)
                return Conflict(new { message = $"A company with name '{request.Name}' already exists" });

            company.Name = request.Name;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated company {CompanyId}", companyId);

        var dto = new CompanyDto(
            company.Id,
            company.Name,
            company.CreatedAt,
            await _context.Sites.CountAsync(s => s.CompanyId == companyId),
            await _context.Users.CountAsync(u => u.CompanyId == companyId)
        );

        return Ok(dto);
    }

    /// <summary>
    /// Delete a company. Only allowed if company has no sites or users.
    /// </summary>
    [HttpDelete("companies/{companyId:int}")]
    public async Task<ActionResult> DeleteCompany(int companyId)
    {
        var company = await _context.Companies.FindAsync(companyId);
        if (company == null)
            return NotFound(new { message = $"Company with ID {companyId} not found" });

        var hasSites = await _context.Sites.AnyAsync(s => s.CompanyId == companyId);
        if (hasSites)
            return BadRequest(new { message = "Cannot delete company with existing sites. Remove sites first." });

        var hasUsers = await _context.Users.AnyAsync(u => u.CompanyId == companyId);
        if (hasUsers)
            return BadRequest(new { message = "Cannot delete company with existing users. Reassign or remove users first." });

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted company {CompanyId}: {CompanyName}", companyId, company.Name);

        return NoContent();
    }

    #endregion

    #region Sites

    /// <summary>
    /// Get all sites with summary info.
    /// </summary>
    [HttpGet("sites")]
    public async Task<ActionResult<IEnumerable<AdminSiteDto>>> GetSites([FromQuery] int? companyId = null)
    {
        var query = _context.Sites.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(s => s.CompanyId == companyId.Value);

        var sites = await query
            .Include(s => s.Company)
            .Select(s => new AdminSiteDto(
                s.Id,
                s.CompanyId,
                s.Company.Name,
                s.SiteName,
                s.Location,
                s.TimeZone,
                s.Status,
                s.OnboardingNotes,
                s.ActivatedAt,
                s.AutoInferenceEnabled,
                s.AutoRetrainingEnabled,
                _context.UserSiteAccesses.Count(usa => usa.SiteId == s.Id),
                s.CreatedAt,
                s.UpdatedAt
            ))
            .OrderBy(s => s.CompanyName)
            .ThenBy(s => s.SiteName)
            .ToListAsync();

        return Ok(sites);
    }

    /// <summary>
    /// Get a specific site by ID.
    /// </summary>
    [HttpGet("sites/{siteId:int}")]
    public async Task<ActionResult<AdminSiteDto>> GetSite(int siteId)
    {
        var site = await _context.Sites
            .Include(s => s.Company)
            .Where(s => s.Id == siteId)
            .Select(s => new AdminSiteDto(
                s.Id,
                s.CompanyId,
                s.Company.Name,
                s.SiteName,
                s.Location,
                s.TimeZone,
                s.Status,
                s.OnboardingNotes,
                s.ActivatedAt,
                s.AutoInferenceEnabled,
                s.AutoRetrainingEnabled,
                _context.UserSiteAccesses.Count(usa => usa.SiteId == s.Id),
                s.CreatedAt,
                s.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        if (site == null)
            return NotFound(new { message = $"Site with ID {siteId} not found" });

        return Ok(site);
    }

    /// <summary>
    /// Create a new site for a company.
    /// </summary>
    [HttpPost("sites")]
    public async Task<ActionResult<AdminSiteDto>> CreateSite([FromBody] CreateSiteRequest request)
    {
        var company = await _context.Companies.FindAsync(request.CompanyId);
        if (company == null)
            return BadRequest(new { message = $"Company with ID {request.CompanyId} not found" });

        // Check for duplicate site name within company
        var duplicate = await _context.Sites
            .FirstOrDefaultAsync(s => s.CompanyId == request.CompanyId &&
                                       s.SiteName.ToLower() == request.SiteName.ToLower());
        if (duplicate != null)
            return Conflict(new { message = $"A site named '{request.SiteName}' already exists for this company" });

        var site = new Site
        {
            CompanyId = request.CompanyId,
            Company = company,
            SiteName = request.SiteName,
            Location = request.Location,
            TimeZone = request.TimeZone ?? "UTC",
            ConfigJson = request.ConfigJson ?? "{}",
            Status = SiteStatus.PendingSetup,
            OnboardingNotes = request.OnboardingNotes,
            AutoInferenceEnabled = request.AutoInferenceEnabled,
            AutoRetrainingEnabled = request.AutoRetrainingEnabled,
            RetrainingFrequencyDays = request.RetrainingFrequencyDays,
            TrainOnEveryUpload = request.TrainOnEveryUpload,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created site {SiteId}: {SiteName} for company {CompanyId}",
            site.Id, site.SiteName, request.CompanyId);

        var dto = new AdminSiteDto(
            site.Id,
            site.CompanyId,
            company.Name,
            site.SiteName,
            site.Location,
            site.TimeZone,
            site.Status,
            site.OnboardingNotes,
            site.ActivatedAt,
            site.AutoInferenceEnabled,
            site.AutoRetrainingEnabled,
            0,
            site.CreatedAt,
            site.UpdatedAt
        );

        return CreatedAtAction(nameof(GetSite), new { siteId = site.Id }, dto);
    }

    /// <summary>
    /// Update a site.
    /// </summary>
    [HttpPatch("sites/{siteId:int}")]
    public async Task<ActionResult<AdminSiteDto>> UpdateSite(int siteId, [FromBody] UpdateSiteRequest request)
    {
        var site = await _context.Sites
            .Include(s => s.Company)
            .FirstOrDefaultAsync(s => s.Id == siteId);

        if (site == null)
            return NotFound(new { message = $"Site with ID {siteId} not found" });

        if (!string.IsNullOrWhiteSpace(request.SiteName))
        {
            // Check for duplicate name within company
            var duplicate = await _context.Sites
                .FirstOrDefaultAsync(s => s.Id != siteId &&
                                           s.CompanyId == site.CompanyId &&
                                           s.SiteName.ToLower() == request.SiteName.ToLower());
            if (duplicate != null)
                return Conflict(new { message = $"A site named '{request.SiteName}' already exists for this company" });

            site.SiteName = request.SiteName;
        }

        if (request.Location != null) site.Location = request.Location;
        if (request.TimeZone != null) site.TimeZone = request.TimeZone;
        if (request.ConfigJson != null) site.ConfigJson = request.ConfigJson;
        if (request.OnboardingNotes != null) site.OnboardingNotes = request.OnboardingNotes;
        if (request.Status.HasValue)
        {
            var previousStatus = site.Status;
            site.Status = request.Status.Value;
            
            // Set ActivatedAt when transitioning to Active
            if (request.Status.Value == SiteStatus.Active && previousStatus != SiteStatus.Active)
            {
                site.ActivatedAt = DateTime.UtcNow;
            }
        }
        if (request.AutoInferenceEnabled.HasValue) site.AutoInferenceEnabled = request.AutoInferenceEnabled.Value;
        if (request.AutoRetrainingEnabled.HasValue) site.AutoRetrainingEnabled = request.AutoRetrainingEnabled.Value;
        if (request.RetrainingFrequencyDays.HasValue) site.RetrainingFrequencyDays = request.RetrainingFrequencyDays;
        if (request.TrainOnEveryUpload.HasValue) site.TrainOnEveryUpload = request.TrainOnEveryUpload.Value;

        site.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated site {SiteId}", siteId);

        var dto = new AdminSiteDto(
            site.Id,
            site.CompanyId,
            site.Company.Name,
            site.SiteName,
            site.Location,
            site.TimeZone,
            site.Status,
            site.OnboardingNotes,
            site.ActivatedAt,
            site.AutoInferenceEnabled,
            site.AutoRetrainingEnabled,
            await _context.UserSiteAccesses.CountAsync(usa => usa.SiteId == siteId),
            site.CreatedAt,
            site.UpdatedAt
        );

        return Ok(dto);
    }

    /// <summary>
    /// Update a site's onboarding status.
    /// </summary>
    [HttpPatch("sites/{siteId:int}/status")]
    public async Task<ActionResult<AdminSiteDto>> UpdateSiteStatus(
        int siteId,
        [FromBody] UpdateSiteStatusRequest request)
    {
        var site = await _context.Sites
            .Include(s => s.Company)
            .FirstOrDefaultAsync(s => s.Id == siteId);

        if (site == null)
            return NotFound(new { message = $"Site with ID {siteId} not found" });

        var previousStatus = site.Status;
        site.Status = request.Status;

        // Set ActivatedAt when transitioning to Active
        if (request.Status == SiteStatus.Active && previousStatus != SiteStatus.Active)
        {
            site.ActivatedAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            site.OnboardingNotes = request.Notes;
        }

        site.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated site {SiteId} status from {OldStatus} to {NewStatus}",
            siteId, previousStatus, request.Status);

        var dto = new AdminSiteDto(
            site.Id,
            site.CompanyId,
            site.Company.Name,
            site.SiteName,
            site.Location,
            site.TimeZone,
            site.Status,
            site.OnboardingNotes,
            site.ActivatedAt,
            site.AutoInferenceEnabled,
            site.AutoRetrainingEnabled,
            await _context.UserSiteAccesses.CountAsync(usa => usa.SiteId == siteId),
            site.CreatedAt,
            site.UpdatedAt
        );

        return Ok(dto);
    }

    /// <summary>
    /// Delete a site. This will cascade delete all related data.
    /// </summary>
    [HttpDelete("sites/{siteId:int}")]
    public async Task<ActionResult> DeleteSite(int siteId)
    {
        var site = await _context.Sites.FindAsync(siteId);
        if (site == null)
            return NotFound(new { message = $"Site with ID {siteId} not found" });

        // Log warning about cascade delete
        var dataRowCount = await _context.DataRows.CountAsync(d => d.SiteId == siteId);
        if (dataRowCount > 0)
        {
            _logger.LogWarning("Deleting site {SiteId} with {DataRowCount} data rows", siteId, dataRowCount);
        }

        _context.Sites.Remove(site);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted site {SiteId}: {SiteName}", siteId, site.SiteName);

        return NoContent();
    }

    #endregion

    #region Users

    /// <summary>
    /// Get all users with summary info.
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers([FromQuery] int? companyId = null)
    {
        var query = _context.Users.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(u => u.CompanyId == companyId.Value);

        var users = await query
            .Include(u => u.Company)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email,
                u.Name,
                u.CompanyId,
                u.Company != null ? u.Company.Name : null,
                u.Role,
                u.IsIndividual,
                u.IdpSub != null,
                _context.UserSiteAccesses.Count(usa => usa.UserId == u.Id),
                u.CreatedAt,
                u.LastLogin
            ))
            .OrderBy(u => u.Email)
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get a specific user by ID.
    /// </summary>
    [HttpGet("users/{userId:int}")]
    public async Task<ActionResult<AdminUserDto>> GetUser(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Where(u => u.Id == userId)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email,
                u.Name,
                u.CompanyId,
                u.Company != null ? u.Company.Name : null,
                u.Role,
                u.IsIndividual,
                u.IdpSub != null,
                _context.UserSiteAccesses.Count(usa => usa.UserId == u.Id),
                u.CreatedAt,
                u.LastLogin
            ))
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = $"User with ID {userId} not found" });

        return Ok(user);
    }

    /// <summary>
    /// Create a new user (manual registration).
    /// The user will be linked to an IDP account when they first log in.
    /// </summary>
    [HttpPost("users")]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        // Check for duplicate email
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
            return Conflict(new { message = $"A user with email '{request.Email}' already exists" });

        // Validate company if provided
        Company? company = null;
        if (request.CompanyId.HasValue)
        {
            company = await _context.Companies.FindAsync(request.CompanyId.Value);
            if (company == null)
                return BadRequest(new { message = $"Company with ID {request.CompanyId} not found" });
        }

        var user = new User
        {
            Email = request.Email.ToLower(),
            Name = request.Name,
            CompanyId = request.CompanyId,
            Company = company,
            Role = request.Role,
            IsIndividual = request.IsIndividual || !request.CompanyId.HasValue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created user {UserId}: {UserEmail} (Company: {CompanyId})",
            user.Id, user.Email, user.CompanyId);

        var dto = new AdminUserDto(
            user.Id,
            user.Email,
            user.Name,
            user.CompanyId,
            company?.Name,
            user.Role,
            user.IsIndividual,
            false,
            0,
            user.CreatedAt,
            user.LastLogin
        );

        return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, dto);
    }

    /// <summary>
    /// Update a user.
    /// </summary>
    [HttpPatch("users/{userId:int}")]
    public async Task<ActionResult<AdminUserDto>> UpdateUser(int userId, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = $"User with ID {userId} not found" });

        if (request.Name != null) user.Name = request.Name;
        if (request.Role.HasValue) user.Role = request.Role.Value;
        if (request.IsIndividual.HasValue) user.IsIndividual = request.IsIndividual.Value;

        if (request.CompanyId.HasValue)
        {
            if (request.CompanyId.Value == 0)
            {
                // Remove from company
                user.CompanyId = null;
                user.Company = null;
                user.IsIndividual = true;
            }
            else
            {
                var company = await _context.Companies.FindAsync(request.CompanyId.Value);
                if (company == null)
                    return BadRequest(new { message = $"Company with ID {request.CompanyId} not found" });

                user.CompanyId = request.CompanyId.Value;
                user.Company = company;
            }
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", userId);

        var dto = new AdminUserDto(
            user.Id,
            user.Email,
            user.Name,
            user.CompanyId,
            user.Company?.Name,
            user.Role,
            user.IsIndividual,
            user.IdpSub != null,
            await _context.UserSiteAccesses.CountAsync(usa => usa.UserId == userId),
            user.CreatedAt,
            user.LastLogin
        );

        return Ok(dto);
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    [HttpDelete("users/{userId:int}")]
    public async Task<ActionResult> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = $"User with ID {userId} not found" });

        // Remove all site access first
        var siteAccesses = await _context.UserSiteAccesses
            .Where(usa => usa.UserId == userId)
            .ToListAsync();

        _context.UserSiteAccesses.RemoveRange(siteAccesses);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted user {UserId}: {UserEmail}", userId, user.Email);

        return NoContent();
    }

    #endregion

    #region Site Access

    /// <summary>
    /// Get all users with access to a specific site.
    /// </summary>
    [HttpGet("sites/{siteId:int}/access")]
    public async Task<ActionResult<IEnumerable<SiteAccessDto>>> GetSiteAccess(int siteId)
    {
        var site = await _context.Sites.FindAsync(siteId);
        if (site == null)
            return NotFound(new { message = $"Site with ID {siteId} not found" });

        var accesses = await _context.UserSiteAccesses
            .Include(usa => usa.User)
            .Include(usa => usa.Site)
            .Where(usa => usa.SiteId == siteId)
            .Select(usa => new SiteAccessDto(
                usa.UserId,
                usa.User.Email,
                usa.User.Name,
                usa.SiteId,
                usa.Site.SiteName,
                usa.RoleOnSite
            ))
            .OrderBy(a => a.UserEmail)
            .ToListAsync();

        return Ok(accesses);
    }

    /// <summary>
    /// Grant a user access to a site with a specific role.
    /// </summary>
    [HttpPost("sites/{siteId:int}/access")]
    public async Task<ActionResult<SiteAccessDto>> GrantSiteAccess(int siteId, [FromBody] GrantSiteAccessRequest request)
    {
        var site = await _context.Sites.FindAsync(siteId);
        if (site == null)
            return NotFound(new { message = $"Site with ID {siteId} not found" });

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            return NotFound(new { message = $"User with ID {request.UserId} not found" });

        var existingAccess = await _context.UserSiteAccesses
            .FirstOrDefaultAsync(usa => usa.UserId == request.UserId && usa.SiteId == siteId);

        if (existingAccess != null)
        {
            // Update existing access
            existingAccess.RoleOnSite = request.Role;
            _logger.LogInformation("Updated site access: User {UserId} -> Site {SiteId} with role {Role}",
                request.UserId, siteId, request.Role);
        }
        else
        {
            // Create new access
            var access = new UserSiteAccess
            {
                UserId = request.UserId,
                User = user,
                SiteId = siteId,
                Site = site,
                RoleOnSite = request.Role
            };
            _context.UserSiteAccesses.Add(access);
            _logger.LogInformation("Granted site access: User {UserId} -> Site {SiteId} with role {Role}",
                request.UserId, siteId, request.Role);
        }

        await _context.SaveChangesAsync();

        var dto = new SiteAccessDto(
            request.UserId,
            user.Email,
            user.Name,
            siteId,
            site.SiteName,
            request.Role
        );

        return Ok(dto);
    }

    /// <summary>
    /// Revoke a user's access to a site.
    /// </summary>
    [HttpDelete("sites/{siteId:int}/access/{userId:int}")]
    public async Task<ActionResult> RevokeSiteAccess(int siteId, int userId)
    {
        var access = await _context.UserSiteAccesses
            .FirstOrDefaultAsync(usa => usa.UserId == userId && usa.SiteId == siteId);

        if (access == null)
            return NotFound(new { message = $"User {userId} does not have access to site {siteId}" });

        _context.UserSiteAccesses.Remove(access);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Revoked site access: User {UserId} from Site {SiteId}", userId, siteId);

        return NoContent();
    }

    /// <summary>
    /// Get all site accesses for a specific user.
    /// </summary>
    [HttpGet("users/{userId:int}/sites")]
    public async Task<ActionResult<IEnumerable<SiteAccessDto>>> GetUserSites(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = $"User with ID {userId} not found" });

        var accesses = await _context.UserSiteAccesses
            .Include(usa => usa.User)
            .Include(usa => usa.Site)
            .Where(usa => usa.UserId == userId)
            .Select(usa => new SiteAccessDto(
                usa.UserId,
                usa.User.Email,
                usa.User.Name,
                usa.SiteId,
                usa.Site.SiteName,
                usa.RoleOnSite
            ))
            .OrderBy(a => a.SiteName)
            .ToListAsync();

        return Ok(accesses);
    }

    #endregion
}
