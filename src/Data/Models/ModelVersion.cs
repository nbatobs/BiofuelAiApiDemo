namespace Data.Models;

public class ModelVersion
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public required Site Site { get; set; }
    public required string BlobStoragePath { get; set; }
    public string? ModelFormat { get; set; } // e.g. "ONNX", "pickle", "TensorFlow SavedModel"
    public string? ModelFramework { get; set; } // e.g. "PyTorch", "scikit-learn", "TensorFlow"
    public DateTime? TrainedAt { get; set; }
    public DateTime? TrainingDataStart { get; set; }
    public DateTime? TrainingDataEnd { get; set; }
    public string? MetricsJson { get; set; }
    public bool IsActive { get; set; }
    public decimal VersionNumber { get; set; }
}
