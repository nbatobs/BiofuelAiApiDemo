using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Admin;

public record CreateSiteRequest(
    [Required]
    int CompanyId,
    
    [Required]
    [StringLength(200, MinimumLength = 2)]
    string SiteName,
    
    string? Location,
    
    string? TimeZone,
    
    string? ConfigJson,
    
    string? OnboardingNotes,
    
    bool AutoInferenceEnabled = false,
    
    bool AutoRetrainingEnabled = false,
    
    int? RetrainingFrequencyDays = null,
    
    bool TrainOnEveryUpload = false
);
