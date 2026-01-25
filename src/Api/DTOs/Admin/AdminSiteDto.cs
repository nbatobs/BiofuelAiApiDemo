namespace Api.DTOs.Admin;

public record AdminSiteDto(
    int Id,
    int CompanyId,
    string CompanyName,
    string SiteName,
    string? Location,
    string? TimeZone,
    bool AutoInferenceEnabled,
    bool AutoRetrainingEnabled,
    int UserCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
