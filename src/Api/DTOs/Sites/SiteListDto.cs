using Data.Models.Enums;

namespace Api.DTOs.Sites;

/// <summary>
/// Summary information for a site in list views.
/// </summary>
public class SiteListDto
{
    public int Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public SiteRole? UserRole { get; set; }
    public DateTime? LastDataUpload { get; set; }
    public bool AutoInferenceEnabled { get; set; }
    public bool AutoRetrainingEnabled { get; set; }
}
