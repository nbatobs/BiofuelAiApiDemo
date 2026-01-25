using System.ComponentModel.DataAnnotations;
using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record GrantSiteAccessRequest(
    [Required]
    int UserId,
    
    [Required]
    SiteRole Role
);
