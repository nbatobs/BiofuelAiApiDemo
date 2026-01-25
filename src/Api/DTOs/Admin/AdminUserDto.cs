using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record AdminUserDto(
    int Id,
    string Email,
    string? Name,
    int? CompanyId,
    string? CompanyName,
    UserRole Role,
    bool IsIndividual,
    bool IsLinkedToIdp,
    int SiteAccessCount,
    DateTime CreatedAt,
    DateTime? LastLogin
);
