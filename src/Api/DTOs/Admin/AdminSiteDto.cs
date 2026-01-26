using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record AdminSiteDto(
    int Id,
    int CompanyId,
    string CompanyName,
    string SiteName,
    string? Location,
    string? TimeZone,
    SiteStatus Status,
    string? OnboardingNotes,
    DateTime? ActivatedAt,
    bool AutoInferenceEnabled,
    bool AutoRetrainingEnabled,
    int UserCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
