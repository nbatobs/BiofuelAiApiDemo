using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using Data.Models.Enums;
using Api.DTOs.Mapping;
using Api.Interfaces;
using Api.Services;

namespace Api.Controllers;

/// <summary>
/// Controller for handling Excel data uploads and column mapping workflow.
/// Supports the initial data setup workflow where users upload Excel files,
/// the system suggests column mappings, and admins confirm the mappings.
/// </summary>
[ApiController]
[Route("api/sites/{siteId:int}/upload")]
[Authorize]
public class DataUploadController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHeaderMappingService _mappingService;
    private readonly ISiteAuthorizationService _siteAuthService;
    private readonly ILogger<DataUploadController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataUploadController(
        AppDbContext context,
        IHeaderMappingService mappingService,
        ISiteAuthorizationService siteAuthService,
        ILogger<DataUploadController> logger)
    {
        _context = context;
        _mappingService = mappingService;
        _siteAuthService = siteAuthService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Upload an Excel file for initial data setup.
    /// The file will be analyzed and column mapping suggestions will be returned.
    /// </summary>
    /// <param name="siteId">The site to upload data for.</param>
    /// <param name="file">The Excel file to upload.</param>
    /// <param name="useAi">Whether to use AI for semantic matching (default: true).</param>
    /// <returns>Upload ID and mapping suggestions.</returns>
    [HttpPost("initial")]
    [ProducesResponseType(typeof(InitialUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<ActionResult<InitialUploadResponse>> UploadInitialData(
        int siteId,
        IFormFile file,
        [FromQuery] bool useAi = true)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return BadRequest(new InitialUploadResponse
            {
                Success = false,
                ErrorMessage = "No file provided."
            });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new InitialUploadResponse
            {
                Success = false,
                ErrorMessage = "Only Excel files (.xlsx, .xls) are supported."
            });
        }

        // Authorize access to site (Operator or higher can upload)
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var hasAccess = await _siteAuthService.HasSiteRoleAsync(userId.Value, siteId, SiteRole.Operator, SiteRole.SiteAdmin);
        if (!hasAccess)
            return Forbid();

        // Verify site exists
        var site = await _context.Sites.FindAsync(siteId);
        if (site == null)
            return NotFound(new { message = "Site not found." });

        try
        {
            // Store the file (in production, this would go to blob storage)
            var blobPath = await SaveFileToBlobStorageAsync(file, siteId);

            // Create pending upload record
            var pendingUpload = new PendingSiteUpload
            {
                SiteId = siteId,
                Site = site,
                UploadedByUserId = userId.Value,
                UploadedBy = (await _context.Users.FindAsync(userId.Value))!,
                FileName = file.FileName,
                BlobPath = blobPath,
                Status = PendingUploadStatus.Processing,
                UploadedAt = DateTime.UtcNow
            };

            _context.PendingSiteUploads.Add(pendingUpload);
            await _context.SaveChangesAsync();

            // Call Python API to get mapping suggestions
            ExcelMappingResultDto? mappingResult = null;
            try
            {
                using var stream = file.OpenReadStream();
                mappingResult = await _mappingService.MapExcelHeadersAsync(
                    stream, file.FileName, useAi);

                if (mappingResult != null)
                {
                    pendingUpload.MappingResultJson = JsonSerializer.Serialize(mappingResult, _jsonOptions);
                    pendingUpload.Status = PendingUploadStatus.AwaitingConfirmation;
                }
                else
                {
                    pendingUpload.Status = PendingUploadStatus.Failed;
                    pendingUpload.ErrorMessage = "Failed to analyze Excel file headers.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to map headers for file {FileName}", file.FileName);
                pendingUpload.Status = PendingUploadStatus.Failed;
                pendingUpload.ErrorMessage = "Failed to analyze Excel file headers.";
            }

            await _context.SaveChangesAsync();

            // Update site status if this is the first upload
            if (site.Status == SiteStatus.PendingSetup)
            {
                site.Status = SiteStatus.DataUploaded;
                await _context.SaveChangesAsync();
            }

            return Ok(new InitialUploadResponse
            {
                Success = mappingResult != null,
                PendingUploadId = pendingUpload.Id,
                FileName = file.FileName,
                MappingResult = mappingResult,
                ErrorMessage = pendingUpload.ErrorMessage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process upload for site {SiteId}", siteId);
            return StatusCode(500, new InitialUploadResponse
            {
                Success = false,
                ErrorMessage = "An error occurred processing the upload."
            });
        }
    }

    /// <summary>
    /// Get all pending uploads for a site awaiting mapping confirmation.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <returns>List of pending uploads with their mapping suggestions.</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<PendingUploadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PendingUploadDto>>> GetPendingUploads(int siteId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var hasAccess = await _siteAuthService.HasSiteAccessAsync(userId.Value, siteId);
        if (!hasAccess)
            return Forbid();

        var pendingUploads = await _context.PendingSiteUploads
            .Where(p => p.SiteId == siteId && p.Status == PendingUploadStatus.AwaitingConfirmation)
            .Include(p => p.Site)
            .Include(p => p.UploadedBy)
            .OrderByDescending(p => p.UploadedAt)
            .ToListAsync();

        var dtos = pendingUploads.Select(p => new PendingUploadDto
        {
            Id = p.Id,
            SiteId = p.SiteId,
            SiteName = p.Site.SiteName,
            FileName = p.FileName,
            BlobPath = p.BlobPath,
            MappingResult = string.IsNullOrEmpty(p.MappingResultJson)
                ? null
                : JsonSerializer.Deserialize<ExcelMappingResultDto>(p.MappingResultJson, _jsonOptions),
            UploadedAt = p.UploadedAt,
            UploadedByUserId = p.UploadedByUserId,
            UploadedByUserName = p.UploadedBy.Name,
            Status = p.Status.ToString()
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific pending upload by ID.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <param name="uploadId">The pending upload ID.</param>
    /// <returns>The pending upload details.</returns>
    [HttpGet("pending/{uploadId:int}")]
    [ProducesResponseType(typeof(PendingUploadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PendingUploadDto>> GetPendingUpload(int siteId, int uploadId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var hasAccess = await _siteAuthService.HasSiteAccessAsync(userId.Value, siteId);
        if (!hasAccess)
            return Forbid();

        var pendingUpload = await _context.PendingSiteUploads
            .Include(p => p.Site)
            .Include(p => p.UploadedBy)
            .FirstOrDefaultAsync(p => p.Id == uploadId && p.SiteId == siteId);

        if (pendingUpload == null)
            return NotFound(new { message = "Pending upload not found." });

        return Ok(new PendingUploadDto
        {
            Id = pendingUpload.Id,
            SiteId = pendingUpload.SiteId,
            SiteName = pendingUpload.Site.SiteName,
            FileName = pendingUpload.FileName,
            BlobPath = pendingUpload.BlobPath,
            MappingResult = string.IsNullOrEmpty(pendingUpload.MappingResultJson)
                ? null
                : JsonSerializer.Deserialize<ExcelMappingResultDto>(pendingUpload.MappingResultJson, _jsonOptions),
            UploadedAt = pendingUpload.UploadedAt,
            UploadedByUserId = pendingUpload.UploadedByUserId,
            UploadedByUserName = pendingUpload.UploadedBy.Name,
            Status = pendingUpload.Status.ToString()
        });
    }

    /// <summary>
    /// Confirm column mappings for a pending upload.
    /// This creates/updates the site's data schema and processes the data.
    /// Admin or SiteAdmin role required.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <param name="request">The confirmed mappings.</param>
    /// <returns>Confirmation response.</returns>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ConfirmMappingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfirmMappingsResponse>> ConfirmMappings(
        int siteId,
        [FromBody] ConfirmMappingsRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Require Admin role for confirming mappings
        var hasAccess = await _siteAuthService.HasSiteRoleAsync(userId.Value, siteId, SiteRole.SiteAdmin);
        if (!hasAccess)
            return Forbid();

        // Find the pending upload
        var pendingUpload = await _context.PendingSiteUploads
            .Include(p => p.Site)
            .FirstOrDefaultAsync(p => p.Id == request.PendingUploadId && p.SiteId == siteId);

        if (pendingUpload == null)
            return NotFound(new { message = "Pending upload not found." });

        if (pendingUpload.Status != PendingUploadStatus.AwaitingConfirmation)
        {
            return BadRequest(new ConfirmMappingsResponse
            {
                Success = false,
                Message = $"Upload is not awaiting confirmation. Current status: {pendingUpload.Status}"
            });
        }

        if (request.Mappings == null || !request.Mappings.Any())
        {
            return BadRequest(new ConfirmMappingsResponse
            {
                Success = false,
                Message = "At least one column mapping is required."
            });
        }

        try
        {
            // Create or update the site's data schema
            var schemaDefinition = new Dictionary<string, string>();
            foreach (var mapping in request.Mappings)
            {
                if (!string.IsNullOrWhiteSpace(mapping.CanonicalColumn))
                {
                    schemaDefinition[mapping.UserColumn] = mapping.CanonicalColumn;
                }
            }

            // Get the next version number for this site's schema
            var latestVersion = await _context.SiteDataSchemas
                .Where(s => s.SiteId == siteId)
                .MaxAsync(s => (decimal?)s.VersionNumber) ?? 0m;

            var newSchema = new SiteDataSchema
            {
                SiteId = siteId,
                Site = pendingUpload.Site,
                VersionNumber = latestVersion + 1m,
                SchemaDefinition = JsonSerializer.Serialize(schemaDefinition, _jsonOptions),
                EffectiveFrom = DateTime.UtcNow,
                ChangeDescription = $"Initial schema from file: {pendingUpload.FileName}",
                CreatedById = userId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.SiteDataSchemas.Add(newSchema);
            await _context.SaveChangesAsync();

            // Update the pending upload status
            pendingUpload.Status = PendingUploadStatus.Confirmed;
            pendingUpload.ConfirmedAt = DateTime.UtcNow;
            pendingUpload.ConfirmedByUserId = userId.Value;
            pendingUpload.ResultingSchemaVersionId = newSchema.Id;

            // Update site status
            var site = pendingUpload.Site;
            if (site.Status == SiteStatus.PendingSetup || site.Status == SiteStatus.DataUploaded)
            {
                site.Status = SiteStatus.SchemaConfigured;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Mappings confirmed for site {SiteId} by user {UserId}. Schema version {Version} created.",
                siteId, userId.Value, newSchema.VersionNumber);

            return Ok(new ConfirmMappingsResponse
            {
                Success = true,
                SiteId = siteId,
                SchemaVersionId = newSchema.Id,
                MappingsCount = request.Mappings.Count,
                Message = $"Successfully created schema version {newSchema.VersionNumber} with {request.Mappings.Count} column mappings."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm mappings for site {SiteId}", siteId);
            return StatusCode(500, new ConfirmMappingsResponse
            {
                Success = false,
                Message = "An error occurred while confirming mappings."
            });
        }
    }

    /// <summary>
    /// Get the list of canonical columns available for mapping.
    /// </summary>
    /// <returns>List of canonical columns with descriptions.</returns>
    [HttpGet("/api/schema/columns")]
    [ProducesResponseType(typeof(CanonicalColumnsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CanonicalColumnsResponse>> GetCanonicalColumns()
    {
        var columns = await _mappingService.GetCanonicalColumnsAsync();
        return Ok(new CanonicalColumnsResponse { Columns = columns });
    }

    /// <summary>
    /// Cancel a pending upload.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <param name="uploadId">The pending upload ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("pending/{uploadId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPendingUpload(int siteId, int uploadId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var hasAccess = await _siteAuthService.HasSiteRoleAsync(userId.Value, siteId, SiteRole.SiteAdmin);
        if (!hasAccess)
            return Forbid();

        var pendingUpload = await _context.PendingSiteUploads
            .FirstOrDefaultAsync(p => p.Id == uploadId && p.SiteId == siteId);

        if (pendingUpload == null)
            return NotFound();

        if (pendingUpload.Status == PendingUploadStatus.Confirmed)
        {
            return BadRequest(new { message = "Cannot cancel a confirmed upload." });
        }

        pendingUpload.Status = PendingUploadStatus.Cancelled;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Check the health of the mapping service.
    /// </summary>
    /// <returns>Health status.</returns>
    [HttpGet("/api/mapping/health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckMappingHealth()
    {
        var isHealthy = await _mappingService.IsHealthyAsync();
        if (isHealthy)
        {
            return Ok(new { status = "healthy", service = "python-mapping-api" });
        }
        return StatusCode(503, new { status = "unhealthy", service = "python-mapping-api" });
    }

    // Helper methods

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }

    /// <summary>
    /// Save uploaded file to blob storage.
    /// In production, this would use Azure Blob Storage or similar.
    /// </summary>
    private async Task<string> SaveFileToBlobStorageAsync(IFormFile file, int siteId)
    {
        // For development, save to local storage
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", siteId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsDir, uniqueFileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath; // In production, return blob URL
    }
}
