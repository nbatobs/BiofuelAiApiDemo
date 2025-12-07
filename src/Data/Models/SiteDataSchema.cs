namespace Data.Models;

public class SiteDataSchema
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public required Site Site { get; set; }
    
    public float VersionNumber { get; set; }
    public required string SchemaDefinition { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public string? ChangeDescription { get; set; }
    
    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}