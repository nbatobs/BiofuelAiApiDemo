namespace Data.Models;

public class DataRow
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public required Site Site { get; set; }
    
    public int? SchemaVersionId { get; set; }
    public SiteDataSchema? SchemaVersion { get; set; }
    
    public DateTime Date { get; set; }
    
    // All sensor readings stored as JSONB for flexibility
    // Structure defined by SchemaVersion.SchemaDefinition
    public required string SensorDataJson { get; set; }
    
    public DateTime CreatedAt { get; set; }
}