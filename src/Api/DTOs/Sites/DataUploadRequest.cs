using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Sites;

/// <summary>
/// Request to upload data rows for a site.
/// Supports both single-row daily uploads and batch uploads.
/// </summary>
public class DataUploadRequest
{
    /// <summary>
    /// The data rows to upload. Each row contains a date and sensor readings.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one data row is required")]
    public List<DataRowInput> Rows { get; set; } = new();

    /// <summary>
    /// Optional: Override existing data for the same dates.
    /// If false, duplicate dates will be rejected.
    /// </summary>
    public bool OverwriteExisting { get; set; } = false;

    /// <summary>
    /// Optional: Skip validation and insert data anyway.
    /// Use with caution - data quality issues may affect model accuracy.
    /// </summary>
    public bool SkipValidation { get; set; } = false;
}

/// <summary>
/// A single data row with sensor readings for a specific date.
/// </summary>
public class DataRowInput
{
    /// <summary>
    /// The date for this data row (typically the day the readings were taken).
    /// </summary>
    [Required]
    public DateTime Date { get; set; }

    /// <summary>
    /// Sensor readings as key-value pairs.
    /// Keys should match the site's schema definition.
    /// </summary>
    [Required]
    public Dictionary<string, object?> SensorData { get; set; } = new();
}
