using Data.Models.Enums;

namespace Api.DTOs.Sites;

/// <summary>
/// Detailed site information.
/// </summary>
public class SiteDetailDto
{
    public int Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
    public SiteRole? UserRole { get; set; }
    
    // Onboarding status
    public SiteStatus Status { get; set; }
    public string? OnboardingNotes { get; set; }
    public DateTime? ActivatedAt { get; set; }
    
    // Automation settings
    public bool AutoInferenceEnabled { get; set; }
    public TimeSpan? InferenceSchedule { get; set; }
    public bool AutoRetrainingEnabled { get; set; }
    public int? RetrainingFrequencyDays { get; set; }
    public bool TrainOnEveryUpload { get; set; }
    
    // Power BI integration
    public bool PowerBiEnabled { get; set; }
    public string? PowerBiWorkspaceId { get; set; }
    
    // Current active model
    public ActiveModelDto? ActiveModel { get; set; }
    
    // Current data schema
    public SchemaInfoDto? CurrentSchema { get; set; }
    
    // Upload statistics
    public int TotalUploads { get; set; }
    public DateTime? LastUploadAt { get; set; }
    public int TotalRowsInserted { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
