using Data.Models.Enums;

namespace Data.Models;

/// <summary>
/// Represents an Excel file upload that is awaiting column mapping confirmation.
/// Once mappings are confirmed, this creates/updates the SiteDataSchema and
/// processes the data into DataRows.
/// </summary>
public class PendingSiteUpload
{
    public int Id { get; set; }

    /// <summary>
    /// The site this upload is for.
    /// </summary>
    public int SiteId { get; set; }
    public required Site Site { get; set; }

    /// <summary>
    /// User who uploaded the file.
    /// </summary>
    public int UploadedByUserId { get; set; }
    public required User UploadedBy { get; set; }

    /// <summary>
    /// Original file name.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Path to the file in blob storage.
    /// </summary>
    public required string BlobPath { get; set; }

    /// <summary>
    /// The mapping suggestions from the Python API stored as JSON.
    /// Contains the full ExcelMappingResult.
    /// </summary>
    public string? MappingResultJson { get; set; }

    /// <summary>
    /// Current status of this pending upload.
    /// </summary>
    public PendingUploadStatus Status { get; set; } = PendingUploadStatus.Processing;

    /// <summary>
    /// Error message if processing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When the file was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// When mappings were confirmed (if confirmed).
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Admin user who confirmed the mappings.
    /// </summary>
    public int? ConfirmedByUserId { get; set; }
    public User? ConfirmedBy { get; set; }

    /// <summary>
    /// The resulting schema version created from this upload (if confirmed).
    /// </summary>
    public int? ResultingSchemaVersionId { get; set; }
    public SiteDataSchema? ResultingSchemaVersion { get; set; }
}
