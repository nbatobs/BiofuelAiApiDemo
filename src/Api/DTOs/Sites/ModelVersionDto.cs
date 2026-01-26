namespace Api.DTOs.Sites;

/// <summary>
/// Model version information.
/// </summary>
public class ModelVersionDto
{
    public int Id { get; set; }
    public decimal VersionNumber { get; set; }
    public DateTime? TrainedAt { get; set; }
    public DateTime? TrainingDataStart { get; set; }
    public DateTime? TrainingDataEnd { get; set; }
    public string? MetricsJson { get; set; }
    public bool IsActive { get; set; }
    public string BlobStoragePath { get; set; } = string.Empty;
}
