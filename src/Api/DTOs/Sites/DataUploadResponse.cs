namespace Api.DTOs.Sites;

/// <summary>
/// Response after uploading data to a site.
/// </summary>
public class DataUploadResponse
{
    /// <summary>
    /// The upload record ID for tracking.
    /// </summary>
    public int UploadId { get; set; }

    /// <summary>
    /// Whether the upload was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of rows that were parsed from the request.
    /// </summary>
    public int RowsParsed { get; set; }

    /// <summary>
    /// Number of rows successfully inserted into the database.
    /// </summary>
    public int RowsInserted { get; set; }

    /// <summary>
    /// Number of rows that were updated (when OverwriteExisting is true).
    /// </summary>
    public int RowsUpdated { get; set; }

    /// <summary>
    /// Number of rows that were skipped (duplicates when OverwriteExisting is false).
    /// </summary>
    public int RowsSkipped { get; set; }

    /// <summary>
    /// Validation warnings (non-fatal issues with the data).
    /// </summary>
    public List<ValidationWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Validation errors (if any rows failed validation).
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();

    /// <summary>
    /// Whether automatic inference was triggered after this upload.
    /// </summary>
    public bool InferenceTriggered { get; set; }

    /// <summary>
    /// The inference request ID if inference was triggered.
    /// </summary>
    public int? InferenceRequestId { get; set; }
}

/// <summary>
/// A validation warning for a specific row.
/// </summary>
public class ValidationWarning
{
    public int RowIndex { get; set; }
    public DateTime Date { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// A validation error for a specific row.
/// </summary>
public class ValidationError
{
    public int RowIndex { get; set; }
    public DateTime Date { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
