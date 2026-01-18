using Api.DTOs.Sites;
using Data.Models;

namespace Api.Services;

/// <summary>
/// Service for handling data uploads and validation.
/// </summary>
public interface IDataIngestionService
{
    /// <summary>
    /// Process and store uploaded data rows for a site.
    /// </summary>
    /// <param name="siteId">The site to upload data to.</param>
    /// <param name="userId">The user performing the upload.</param>
    /// <param name="request">The upload request containing data rows.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload response with results and any validation issues.</returns>
    Task<DataUploadResponse> ProcessUploadAsync(
        int siteId, 
        int userId, 
        DataUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate data rows against the site's schema.
    /// </summary>
    /// <param name="siteId">The site ID.</param>
    /// <param name="rows">The data rows to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation warnings and errors.</returns>
    Task<(List<ValidationWarning> Warnings, List<ValidationError> Errors)> ValidateDataAsync(
        int siteId,
        List<DataRowInput> rows,
        CancellationToken cancellationToken = default);
}
