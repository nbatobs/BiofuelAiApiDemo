using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record SiteAccessDto(
    int UserId,
    string UserEmail,
    string? UserName,
    int SiteId,
    string SiteName,
    SiteRole Role
);
