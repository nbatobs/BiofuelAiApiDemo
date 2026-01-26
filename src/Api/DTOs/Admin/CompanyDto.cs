namespace Api.DTOs.Admin;

public record CompanyDto(
    int Id,
    string Name,
    DateTime CreatedAt,
    int SiteCount,
    int UserCount
);
