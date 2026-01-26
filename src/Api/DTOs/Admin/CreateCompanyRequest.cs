using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Admin;

public record CreateCompanyRequest(
    [Required]
    [StringLength(200, MinimumLength = 2)]
    string Name
);
