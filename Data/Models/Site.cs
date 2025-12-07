namespace Data.Models;

public class Site
{
    public int Id { get; set; }
    
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    
    public string SiteName { get; set; }
    public string Location { get; set; }
    public string TimeZone { get; set; } // e.g. "America/New_York"
    
    public int? CurrentSchemaVersionId { get; set; }
    
    // Site configuration
    public string ConfigJson { get; set; } // Equipment IDs, thresholds, custom settings
    
    // Automation settings
    public bool AutoInferenceEnabled { get; set; }
    public TimeSpan? InferenceSchedule { get; set; } // e.g. run at 2 AM daily
    
    public bool AutoRetrainingEnabled { get; set; }
    public int? RetrainingFrequencyDays { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}