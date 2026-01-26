using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record UpdateUserRequest(
    string? Name,
    int? CompanyId,
    UserRole? Role,
    bool? IsIndividual
);
