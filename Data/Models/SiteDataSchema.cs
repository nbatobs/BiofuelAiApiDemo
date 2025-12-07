namespace Data.Models;

public class SiteDataSchema
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public Site Site { get; set; }
    
    public float VersionNumber { get; set; }
    public string SchemaDefinition { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public string ChangeDescription { get; set; }
    
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}