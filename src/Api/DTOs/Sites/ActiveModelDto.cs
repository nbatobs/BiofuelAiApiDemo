namespace Api.DTOs.Sites;

/// <summary>
/// Active model information.
/// </summary>
public class ActiveModelDto
{
    public int Id { get; set; }
    public decimal VersionNumber { get; set; }
    public DateTime? TrainedAt { get; set; }
    public DateTime? TrainingDataStart { get; set; }
    public DateTime? TrainingDataEnd { get; set; }
    public string? MetricsJson { get; set; }
}
