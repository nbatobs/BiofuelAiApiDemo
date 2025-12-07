namespace Data.Models;

using Data.Models.Enums;

public class PredictionResult
{
    public int Id { get; set; }
    
    public int SiteId { get; set; }
    public Site Site { get; set; }

    public int? UserId { get; set; }
    public User User { get; set; }

    public int? ModelVersionId { get; set; }
    public ModelVersion ModelVersion { get; set; }

    public string ScenarioName { get; set; }
    public string InputDataJson { get; set; }
    public string PredictionOutputJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public long DurationMs { get; set; }
    public PredictionStatus Status { get; set; }
}
