namespace Data.Models;

using Data.Models.Enums;

public class Site
{
    public int Id { get; set; }
    
    public int CompanyId { get; set; }
    public required Company Company { get; set; }
    
    public required string SiteName { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; } // e.g. "America/New_York"
    
    // Onboarding status
    public SiteStatus Status { get; set; } = SiteStatus.PendingSetup;
    public string? OnboardingNotes { get; set; }  // Admin notes during setup
    public DateTime? ActivatedAt { get; set; }    // When site went live
    
    public int? CurrentSchemaVersionId { get; set; }
    
    // Site configuration
    public required string ConfigJson { get; set; } // Equipment IDs, thresholds, custom settings
    
    // Automation settings
    public bool AutoInferenceEnabled { get; set; }
    public TimeSpan? InferenceSchedule { get; set; } // e.g. run at 2 AM daily
    
    public bool AutoRetrainingEnabled { get; set; }
    public int? RetrainingFrequencyDays { get; set; }
    public bool TrainOnEveryUpload { get; set; }
    
    // Power BI integration
    public bool PowerBiEnabled { get; set; }
    public string? PowerBiWorkspaceId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}