namespace Data.Models;

using Data.Models.Enums;

public class TrainingJob
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public required Site Site { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public TrainingJobStatus Status { get; set; }
    
    public DateTime TrainingDataStart { get; set; }
    public DateTime TrainingDataEnd { get; set; }
    
    // Result
    public int? ModelVersionId { get; set; }
    public ModelVersion? ModelVersion { get; set; }
    
    public required string ConfigJson { get; set; } // Training hyperparameters, settings
    public string? LogsJson { get; set; } // Training logs, metrics over time
    public string? ErrorMessage { get; set; }
    
    public int? TriggeredById { get; set; }
    public User? TriggeredBy { get; set; }
}
