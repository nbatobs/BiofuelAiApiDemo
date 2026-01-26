using System.ComponentModel.DataAnnotations;
using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record CreateUserRequest(
    [Required]
    [EmailAddress]
    string Email,
    
    string? Name,
    
    int? CompanyId,
    
    UserRole Role = UserRole.User,
    
    bool IsIndividual = false
);
