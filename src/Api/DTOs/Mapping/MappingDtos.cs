namespace Api.DTOs.Mapping;

/// <summary>
/// Response from header mapping service for a single column mapping.
/// </summary>
public class ColumnMappingDto
{
    public string UserColumn { get; set; } = string.Empty;
    public string CanonicalColumn { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string RecommendedAction { get; set; } = string.Empty; // AutoMap, Review, ManualMap
}

/// <summary>
/// Summary of mapping results.
/// </summary>
public class MappingSummaryDto
{
    public int TotalHeaders { get; set; }
    public int AutoMapped { get; set; }
    public int NeedsReview { get; set; }
    public int NeedsManual { get; set; }
}

/// <summary>
/// Mapping results for a single Excel sheet.
/// </summary>
public class SheetMappingDto
{
    public string SheetName { get; set; } = string.Empty;
    public int HeaderRowCount { get; set; }
    public int TotalColumns { get; set; }
    public List<ColumnMappingDto> Mappings { get; set; } = new();
    public SheetMappingSummaryDto Summary { get; set; } = new();
}

public class SheetMappingSummaryDto
{
    public int AutoMapped { get; set; }
    public int NeedsReview { get; set; }
    public int NeedsManual { get; set; }
    public double AutoMappedPercentage { get; set; }
}

/// <summary>
/// Complete mapping response for an Excel file upload.
/// </summary>
public class ExcelMappingResultDto
{
    public string FileName { get; set; } = string.Empty;
    public List<SheetMappingDto> Sheets { get; set; } = new();
    public OverallMappingSummaryDto OverallSummary { get; set; } = new();
}

public class OverallMappingSummaryDto
{
    public int TotalSheets { get; set; }
    public int TotalHeaders { get; set; }
    public int TotalAutoMapped { get; set; }
    public int TotalNeedsReview { get; set; }
    public int TotalNeedsManual { get; set; }
}

/// <summary>
/// Request to confirm column mappings for a site.
/// </summary>
public class ConfirmMappingsRequest
{
    /// <summary>
    /// The pending upload ID to confirm.
    /// </summary>
    public int PendingUploadId { get; set; }

    /// <summary>
    /// The finalized column mappings.
    /// </summary>
    public List<ConfirmedMappingDto> Mappings { get; set; } = new();
}

/// <summary>
/// A single confirmed column mapping.
/// </summary>
public class ConfirmedMappingDto
{
    public string UserColumn { get; set; } = string.Empty;
    public string CanonicalColumn { get; set; } = string.Empty;
}

/// <summary>
/// Response after confirming mappings.
/// </summary>
public class ConfirmMappingsResponse
{
    public bool Success { get; set; }
    public int SiteId { get; set; }
    public int SchemaVersionId { get; set; }
    public int MappingsCount { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Pending upload awaiting mapping confirmation.
/// </summary>
public class PendingUploadDto
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? BlobPath { get; set; }
    public ExcelMappingResultDto? MappingResult { get; set; }
    public DateTime UploadedAt { get; set; }
    public int UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Response after initial Excel upload.
/// </summary>
public class InitialUploadResponse
{
    public bool Success { get; set; }
    public int PendingUploadId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public ExcelMappingResultDto? MappingResult { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// List of available canonical columns for manual mapping.
/// </summary>
public class CanonicalColumnsResponse
{
    public List<CanonicalColumnDto> Columns { get; set; } = new();
}

public class CanonicalColumnDto
{
    public string CanonicalName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool Required { get; set; }
}
