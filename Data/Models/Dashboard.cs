namespace Data.Models;

public class Dashboard
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public Site Site { get; set; }
    
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    
    public string PlotlyConfigJson { get; set; } // Chart types, filters, layout config
    
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastViewedAt { get; set; }
}
