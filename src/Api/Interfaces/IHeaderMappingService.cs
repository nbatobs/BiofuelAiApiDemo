using Api.DTOs.Mapping;

namespace Api.Interfaces;

/// <summary>
/// Service for header mapping operations via the Python API.
/// </summary>
public interface IHeaderMappingService
{
    /// <summary>
    /// Upload an Excel file and get header mapping suggestions.
    /// </summary>
    /// <param name="fileStream">The Excel file stream.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="useAi">Whether to use AI semantic matching.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mapping results for all sheets.</returns>
    Task<ExcelMappingResultDto?> MapExcelHeadersAsync(
        Stream fileStream,
        string fileName,
        bool useAi = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Map a list of headers to canonical columns.
    /// </summary>
    /// <param name="headers">The headers to map.</param>
    /// <param name="useAi">Whether to use AI semantic matching.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mapping results.</returns>
    Task<(List<ColumnMappingDto> Mappings, MappingSummaryDto Summary)?> MapHeadersAsync(
        List<string> headers,
        bool useAi = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the list of available canonical columns.
    /// </summary>
    Task<List<CanonicalColumnDto>> GetCanonicalColumnsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the Python header mapping service is healthy.
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
