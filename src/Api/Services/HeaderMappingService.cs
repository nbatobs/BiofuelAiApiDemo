using System.Net.Http.Headers;
using System.Text.Json;
using Api.DTOs.Mapping;
using Api.Interfaces;

namespace Api.Services;

/// <summary>
/// Service for communicating with the Python header mapping API.
/// </summary>
public class HeaderMappingService : IHeaderMappingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HeaderMappingService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HeaderMappingService(HttpClient httpClient, ILogger<HeaderMappingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ExcelMappingResultDto?> MapExcelHeadersAsync(
        Stream fileStream,
        string fileName,
        bool useAi = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync(
                $"/mapping/excel?use_ai={useAi.ToString().ToLower()}",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Header mapping API returned {StatusCode}: {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<PythonExcelMappingResponse>(jsonResponse, _jsonOptions);

            if (result == null) return null;

            // Map Python API response to our DTO
            return new ExcelMappingResultDto
            {
                FileName = result.FileName ?? fileName,
                Sheets = result.Sheets?.Select(s => new SheetMappingDto
                {
                    SheetName = s.SheetName ?? "",
                    HeaderRowCount = s.HeaderRowCount,
                    TotalColumns = s.TotalColumns,
                    Mappings = s.Mappings?.Select(m => new ColumnMappingDto
                    {
                        UserColumn = m.UserColumn ?? "",
                        CanonicalColumn = m.CanonicalColumn ?? "",
                        Confidence = m.Confidence,
                        RecommendedAction = m.RecommendedAction ?? "ManualMap"
                    }).ToList() ?? new List<ColumnMappingDto>(),
                    Summary = new SheetMappingSummaryDto
                    {
                        AutoMapped = s.Summary?.AutoMapped ?? 0,
                        NeedsReview = s.Summary?.NeedsReview ?? 0,
                        NeedsManual = s.Summary?.NeedsManual ?? 0,
                        AutoMappedPercentage = s.Summary?.AutoMappedPercentage ?? 0
                    }
                }).ToList() ?? new List<SheetMappingDto>(),
                OverallSummary = new OverallMappingSummaryDto
                {
                    TotalSheets = result.OverallSummary?.TotalSheets ?? 0,
                    TotalHeaders = result.OverallSummary?.TotalHeaders ?? 0,
                    TotalAutoMapped = result.OverallSummary?.TotalAutoMapped ?? 0,
                    TotalNeedsReview = result.OverallSummary?.TotalNeedsReview ?? 0,
                    TotalNeedsManual = result.OverallSummary?.TotalNeedsManual ?? 0
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call header mapping API");
            return null;
        }
    }

    public async Task<(List<ColumnMappingDto> Mappings, MappingSummaryDto Summary)?> MapHeadersAsync(
        List<string> headers,
        bool useAi = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { headers, use_ai = useAi };
            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/mapping/headers", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Header mapping API returned {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<PythonHeaderMappingResponse>(jsonResponse, _jsonOptions);

            if (result == null) return null;

            var mappings = result.Mappings?.Select(m => new ColumnMappingDto
            {
                UserColumn = m.UserColumn ?? "",
                CanonicalColumn = m.CanonicalColumn ?? "",
                Confidence = m.Confidence,
                RecommendedAction = m.RecommendedAction ?? "ManualMap"
            }).ToList() ?? new List<ColumnMappingDto>();

            var summary = new MappingSummaryDto
            {
                TotalHeaders = result.Summary?.TotalHeaders ?? 0,
                AutoMapped = result.Summary?.AutoMapped ?? 0,
                NeedsReview = result.Summary?.NeedsReview ?? 0,
                NeedsManual = result.Summary?.NeedsManual ?? 0
            };

            return (mappings, summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call header mapping API");
            return null;
        }
    }

    public async Task<List<CanonicalColumnDto>> GetCanonicalColumnsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/schema", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get canonical columns: {StatusCode}", response.StatusCode);
                return new List<CanonicalColumnDto>();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var schema = JsonSerializer.Deserialize<Dictionary<string, PythonSchemaColumn>>(
                jsonResponse, _jsonOptions);

            if (schema == null) return new List<CanonicalColumnDto>();

            return schema.Values.Select(c => new CanonicalColumnDto
            {
                CanonicalName = c.CanonicalName ?? "",
                Description = c.Description ?? "",
                DataType = c.DataType ?? "",
                Required = c.Required
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get canonical columns");
            return new List<CanonicalColumnDto>();
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Internal DTOs for Python API responses
    private class PythonExcelMappingResponse
    {
        public string? FileName { get; set; }
        public List<PythonSheetMapping>? Sheets { get; set; }
        public PythonOverallSummary? OverallSummary { get; set; }
    }

    private class PythonSheetMapping
    {
        public string? SheetName { get; set; }
        public int HeaderRowCount { get; set; }
        public int TotalColumns { get; set; }
        public List<PythonColumnMapping>? Mappings { get; set; }
        public PythonSheetSummary? Summary { get; set; }
    }

    private class PythonColumnMapping
    {
        public string? UserColumn { get; set; }
        public string? CanonicalColumn { get; set; }
        public double Confidence { get; set; }
        public string? RecommendedAction { get; set; }
    }

    private class PythonSheetSummary
    {
        public int AutoMapped { get; set; }
        public int NeedsReview { get; set; }
        public int NeedsManual { get; set; }
        public double AutoMappedPercentage { get; set; }
    }

    private class PythonOverallSummary
    {
        public int TotalSheets { get; set; }
        public int TotalHeaders { get; set; }
        public int TotalAutoMapped { get; set; }
        public int TotalNeedsReview { get; set; }
        public int TotalNeedsManual { get; set; }
    }

    private class PythonHeaderMappingResponse
    {
        public List<PythonColumnMapping>? Mappings { get; set; }
        public PythonMappingSummary? Summary { get; set; }
    }

    private class PythonMappingSummary
    {
        public int TotalHeaders { get; set; }
        public int AutoMapped { get; set; }
        public int NeedsReview { get; set; }
        public int NeedsManual { get; set; }
    }

    private class PythonSchemaColumn
    {
        public string? CanonicalName { get; set; }
        public string? Description { get; set; }
        public string? DataType { get; set; }
        public bool Required { get; set; }
    }
}
