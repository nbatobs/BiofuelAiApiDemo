using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Admin;

public record UpdateSiteRequest(
    [StringLength(200, MinimumLength = 2)]
    string? SiteName,
    
    string? Location,
    
    string? TimeZone,
    
    string? ConfigJson,
    
    bool? AutoInferenceEnabled,
    
    bool? AutoRetrainingEnabled,
    
    int? RetrainingFrequencyDays,
    
    bool? TrainOnEveryUpload
);
