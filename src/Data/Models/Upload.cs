namespace Data.Models;

using Data.Models.Enums;

public class Upload
{
    public int Id { get; set; }

    public int SiteId { get; set; }
    public required Site Site { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public DateTime UploadedAt { get; set; }
    public required string FileName { get; set; }

    public int RowsParsed { get; set; }
    public int RowsInserted { get; set; }

    public UploadValidationStatus ValidationStatus { get; set; }
    public string? ErrorMessage { get; set; }
}
