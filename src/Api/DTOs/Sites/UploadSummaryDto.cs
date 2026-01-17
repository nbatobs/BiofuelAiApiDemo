using Data.Models.Enums;

namespace Api.DTOs.Sites;

/// <summary>
/// Upload summary information.
/// </summary>
public class UploadSummaryDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int? UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    public UploadValidationStatus ValidationStatus { get; set; }
    public int RowsParsed { get; set; }
    public int RowsInserted { get; set; }
    public string? ErrorMessage { get; set; }
}
