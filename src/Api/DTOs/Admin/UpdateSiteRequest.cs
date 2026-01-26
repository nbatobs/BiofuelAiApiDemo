using System.ComponentModel.DataAnnotations;
using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record UpdateSiteRequest(
    [StringLength(200, MinimumLength = 2)]
    string? SiteName,
    
    string? Location,
    
    string? TimeZone,
    
    string? ConfigJson,
    
    SiteStatus? Status,
    
    string? OnboardingNotes,
    
    bool? AutoInferenceEnabled,
    
    bool? AutoRetrainingEnabled,
    
    int? RetrainingFrequencyDays,
    
    bool? TrainOnEveryUpload
);
