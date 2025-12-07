namespace Data.Models;

public class ModelVersion
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public Site Site { get; set; }
    public string BlobStoragePath { get; set; }
    public DateTime? TrainedAt { get; set; }
    public DateTime? TrainingDataStart { get; set; }
    public DateTime? TrainingDataEnd { get; set; }
    public string MetricsJson { get; set; }
    public bool IsActive { get; set; }
    public float VersionNumber { get; set; }
}
