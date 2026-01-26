using System.ComponentModel.DataAnnotations;
using Data.Models.Enums;

namespace Api.DTOs.Admin;

public record UpdateSiteStatusRequest(
    [Required]
    SiteStatus Status,
    
    string? Notes
);
