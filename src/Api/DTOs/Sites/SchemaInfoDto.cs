namespace Api.DTOs.Sites;

/// <summary>
/// Schema information for a site.
/// </summary>
public class SchemaInfoDto
{
    public int Id { get; set; }
    public decimal VersionNumber { get; set; }
    public string SchemaDefinition { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
}
