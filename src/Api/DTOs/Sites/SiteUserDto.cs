using Data.Models.Enums;

namespace Api.DTOs.Sites;

/// <summary>
/// User with site access information.
/// </summary>
public class SiteUserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public SiteRole RoleOnSite { get; set; }
    public string? CompanyName { get; set; }
}
