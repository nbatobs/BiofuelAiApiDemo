namespace Data.Models;

using Data.Models.Enums;

public class InferenceRequest
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public Site Site { get; set; }
    
    public int? UserId { get; set; }
    public User User { get; set; }
    
    public DateTime RequestedAt { get; set; }
    public InferenceRequestStatus Status { get; set; }
    
    public string InputDataJson { get; set; } // Input data passed to inference
    
    // Link to result when complete
    public int? PredictionResultId { get; set; }
    public PredictionResult PredictionResult { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public string ErrorMessage { get; set; }
}
