using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Admin;

public record UpdateCompanyRequest(
    [StringLength(200, MinimumLength = 2)]
    string? Name
);
