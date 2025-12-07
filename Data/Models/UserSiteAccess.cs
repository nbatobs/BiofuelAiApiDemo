namespace Data.Models;

using Data.Models.Enums;

public class UserSiteAccess
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int SiteId { get; set; }
    public Site Site { get; set; }

    public SiteRole RoleOnSite { get; set; }
}
