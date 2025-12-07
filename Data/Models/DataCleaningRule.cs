namespace Data.Models;

using Data.Models.Enums;

public class DataCleaningRule
{
    public int Id { get; set; }

    public int SiteId { get; set; }
    public Site Site { get; set; }

    public CleaningRuleType RuleType { get; set; }
    public string ConfigJson { get; set; }

    public bool IsActive { get; set; }
    public int Priority { get; set; }

    public int? CreatedById { get; set; }
    public User CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public float VersionNumber { get; set; }
}
