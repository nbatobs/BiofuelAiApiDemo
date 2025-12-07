namespace Data.Models;

using Data.Models.Enums;

public class UserSiteAccess
{
    public int UserId { get; set; }
    public required User User { get; set; }

    public int SiteId { get; set; }
    public required Site Site { get; set; }

    public SiteRole RoleOnSite { get; set; }
}
