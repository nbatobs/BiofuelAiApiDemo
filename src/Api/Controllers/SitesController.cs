using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using Data.Models.Enums;
using Api.Services;
using Api.DTOs.Sites;

namespace Api.Controllers;

/// <summary>
/// Controller for managing site access and information.
/// Users can view sites they have access to and retrieve site details.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SitesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly ISiteAuthorizationService _siteAuthService;
    private readonly IDataIngestionService _dataIngestionService;
    private readonly ILogger<SitesController> _logger;

    public SitesController(
        AppDbContext context,
        IUserService userService,
        ISiteAuthorizationService siteAuthService,
        IDataIngestionService dataIngestionService,
        ILogger<SitesController> logger)
    {
        _context = context;
        _userService = userService;
        _siteAuthService = siteAuthService;
        _dataIngestionService = dataIngestionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all sites the current user has access to.
    /// </summary>
    /// <returns>List of sites with basic information and user's role.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SiteListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<SiteListDto>>> GetMySites()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var accessibleSiteIds = await _siteAuthService.GetUserAccessibleSiteIdsAsync(user.Id);

        var sites = await _context.Sites
            .Where(s => accessibleSiteIds.Contains(s.Id))
            .Include(s => s.Company)
            .Select(s => new SiteListDto
            {
                Id = s.Id,
                SiteName = s.SiteName,
                CompanyName = s.Company.Name,
                Location = s.Location,
                Status = s.Status,
                LastDataUpload = _context.Uploads
                    .Where(u => u.SiteId == s.Id)
                    .OrderByDescending(u => u.UploadedAt)
                    .Select(u => (DateTime?)u.UploadedAt)
                    .FirstOrDefault(),
                AutoInferenceEnabled = s.AutoInferenceEnabled,
                AutoRetrainingEnabled = s.AutoRetrainingEnabled
            })
            .ToListAsync();

        // Get roles for each site
        var userSiteAccesses = await _context.UserSiteAccesses
            .Where(usa => usa.UserId == user.Id && accessibleSiteIds.Contains(usa.SiteId))
            .ToDictionaryAsync(usa => usa.SiteId, usa => usa.RoleOnSite);

        foreach (var site in sites)
        {
            site.UserRole = userSiteAccesses.TryGetValue(site.Id, out var role) 
                ? role 
                : null; // null for SuperUser/Admin without explicit access entry
        }

        _logger.LogInformation("User {UserId} retrieved {Count} accessible sites", user.Id, sites.Count);
        return Ok(sites);
    }

    /// <summary>
    /// Get detailed information for a specific site.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <returns>Detailed site information including active model and schema version.</returns>
    [HttpGet("{siteId:int}")]
    [ProducesResponseType(typeof(SiteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteDetailDto>> GetSite(int siteId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        if (!await _siteAuthService.HasSiteAccessAsync(user.Id, siteId))
            return Forbid();

        var site = await _context.Sites
            .Include(s => s.Company)
            .FirstOrDefaultAsync(s => s.Id == siteId);

        if (site == null)
            return NotFound();

        var userRole = await _siteAuthService.GetUserSiteRoleAsync(user.Id, siteId);

        var activeModel = await _context.ModelVersions
            .Where(mv => mv.SiteId == siteId && mv.IsActive)
            .Select(mv => new ActiveModelDto
            {
                Id = mv.Id,
                VersionNumber = mv.VersionNumber,
                TrainedAt = mv.TrainedAt,
                TrainingDataStart = mv.TrainingDataStart,
                TrainingDataEnd = mv.TrainingDataEnd,
                MetricsJson = mv.MetricsJson
            })
            .FirstOrDefaultAsync();

        var currentSchema = await _context.SiteDataSchemas
            .Where(sds => sds.Id == site.CurrentSchemaVersionId)
            .Select(sds => new SchemaInfoDto
            {
                Id = sds.Id,
                VersionNumber = sds.VersionNumber,
                SchemaDefinition = sds.SchemaDefinition,
                EffectiveFrom = sds.EffectiveFrom
            })
            .FirstOrDefaultAsync();

        var uploadStats = await _context.Uploads
            .Where(u => u.SiteId == siteId)
            .GroupBy(u => 1)
            .Select(g => new
            {
                TotalUploads = g.Count(),
                LastUpload = g.Max(u => u.UploadedAt),
                TotalRowsInserted = g.Sum(u => u.RowsInserted)
            })
            .FirstOrDefaultAsync();

        _logger.LogInformation("User {UserId} retrieved details for site {SiteId}", user.Id, siteId);

        return Ok(new SiteDetailDto
        {
            Id = site.Id,
            SiteName = site.SiteName,
            CompanyId = site.CompanyId,
            CompanyName = site.Company.Name,
            Location = site.Location,
            TimeZone = site.TimeZone,
            UserRole = userRole,
            Status = site.Status,
            OnboardingNotes = site.OnboardingNotes,
            ActivatedAt = site.ActivatedAt,
            AutoInferenceEnabled = site.AutoInferenceEnabled,
            InferenceSchedule = site.InferenceSchedule,
            AutoRetrainingEnabled = site.AutoRetrainingEnabled,
            RetrainingFrequencyDays = site.RetrainingFrequencyDays,
            TrainOnEveryUpload = site.TrainOnEveryUpload,
            PowerBiEnabled = site.PowerBiEnabled,
            PowerBiWorkspaceId = site.PowerBiWorkspaceId,
            ActiveModel = activeModel,
            CurrentSchema = currentSchema,
            TotalUploads = uploadStats?.TotalUploads ?? 0,
            LastUploadAt = uploadStats?.LastUpload,
            TotalRowsInserted = uploadStats?.TotalRowsInserted ?? 0,
            CreatedAt = site.CreatedAt,
            UpdatedAt = site.UpdatedAt
        });
    }

    /// <summary>
    /// Get recent uploads for a site.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <param name="limit">Maximum number of uploads to return (default 20).</param>
    /// <returns>List of recent uploads with status information.</returns>
    [HttpGet("{siteId:int}/uploads")]
    [ProducesResponseType(typeof(IEnumerable<UploadSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UploadSummaryDto>>> GetSiteUploads(
        int siteId,
        [FromQuery] int limit = 20)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        if (!await _siteAuthService.HasSiteAccessAsync(user.Id, siteId))
            return Forbid();

        var uploads = await _context.Uploads
            .Where(u => u.SiteId == siteId)
            .OrderByDescending(u => u.UploadedAt)
            .Take(limit)
            .Select(u => new UploadSummaryDto
            {
                Id = u.Id,
                FileName = u.FileName,
                UploadedAt = u.UploadedAt,
                UploadedByUserId = u.UserId,
                UploadedByUserName = u.User != null ? u.User.Name : null,
                ValidationStatus = u.ValidationStatus,
                RowsParsed = u.RowsParsed,
                RowsInserted = u.RowsInserted,
                ErrorMessage = u.ErrorMessage
            })
            .ToListAsync();

        return Ok(uploads);
    }

    /// <summary>
    /// Upload data rows for a site.
    /// Supports incremental daily uploads - only new or updated data needs to be sent.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <param name="request">The data upload request containing rows.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result with statistics and any validation issues.</returns>
    [HttpPost("{siteId:int}/data")]
    [ProducesResponseType(typeof(DataUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataUploadResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataUploadResponse>> UploadData(
        int siteId,
        [FromBody] DataUploadRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        // Check if user has write access (Operator or higher)
        if (!await _siteAuthService.HasSiteRoleAsync(user.Id, siteId, 
            SiteRole.Owner, SiteRole.SiteAdmin, SiteRole.Operator))
            return Forbid();

        // Verify site exists
        var siteExists = await _context.Sites.AnyAsync(s => s.Id == siteId, cancellationToken);
        if (!siteExists)
            return NotFound($"Site with ID {siteId} not found");

        _logger.LogInformation(
            "User {UserId} uploading {RowCount} rows to site {SiteId}",
            user.Id, request.Rows.Count, siteId);

        var response = await _dataIngestionService.ProcessUploadAsync(
            siteId, user.Id, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get model versions for a site.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <param name="limit">Maximum number of models to return (default 10).</param>
    /// <returns>List of model versions with training information.</returns>
    [HttpGet("{siteId:int}/models")]
    [ProducesResponseType(typeof(IEnumerable<ModelVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ModelVersionDto>>> GetSiteModels(
        int siteId,
        [FromQuery] int limit = 10)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        if (!await _siteAuthService.HasSiteAccessAsync(user.Id, siteId))
            return Forbid();

        var models = await _context.ModelVersions
            .Where(mv => mv.SiteId == siteId)
            .OrderByDescending(mv => mv.VersionNumber)
            .Take(limit)
            .Select(mv => new ModelVersionDto
            {
                Id = mv.Id,
                VersionNumber = mv.VersionNumber,
                TrainedAt = mv.TrainedAt,
                TrainingDataStart = mv.TrainingDataStart,
                TrainingDataEnd = mv.TrainingDataEnd,
                MetricsJson = mv.MetricsJson,
                IsActive = mv.IsActive,
                BlobStoragePath = mv.BlobStoragePath
            })
            .ToListAsync();

        return Ok(models);
    }

    /// <summary>
    /// Get users with access to a site (requires SiteAdmin or higher role).
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <returns>List of users with their roles on this site.</returns>
    [HttpGet("{siteId:int}/users")]
    [ProducesResponseType(typeof(IEnumerable<SiteUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SiteUserDto>>> GetSiteUsers(int siteId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        // Only SiteAdmin or higher can view site users
        if (!await _siteAuthService.HasSiteRoleAsync(user.Id, siteId, SiteRole.Owner, SiteRole.SiteAdmin))
            return Forbid();

        var siteUsers = await _context.UserSiteAccesses
            .Where(usa => usa.SiteId == siteId)
            .Include(usa => usa.User)
            .Select(usa => new SiteUserDto
            {
                UserId = usa.UserId,
                Email = usa.User.Email,
                Name = usa.User.Name,
                RoleOnSite = usa.RoleOnSite,
                CompanyName = usa.User.Company != null ? usa.User.Company.Name : null
            })
            .ToListAsync();

        return Ok(siteUsers);
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var idpSub = User.FindFirst("sub")?.Value ?? User.FindFirst("oid")?.Value;
        var idpIssuer = User.FindFirst("iss")?.Value;

        if (string.IsNullOrEmpty(idpSub) || string.IsNullOrEmpty(idpIssuer))
        {
            _logger.LogWarning("Missing IDP claims in token");
            return null;
        }

        return await _userService.GetUserByIdpSubAsync(idpSub, idpIssuer);
    }
}
