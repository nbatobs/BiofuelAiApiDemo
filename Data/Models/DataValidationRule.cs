namespace Data.Models;

using Data.Models.Enums;

public class DataValidationRule
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public Site Site { get; set; }
    
    public string ColumnName { get; set; }
    public ValidationRuleType RuleType { get; set; }
    public string ConfigJson { get; set; } // JSON config: min/max, regex, allowed values, etc.
    
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    
    public int? CreatedById { get; set; }
    public User CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
