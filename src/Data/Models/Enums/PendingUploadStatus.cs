namespace Data.Models.Enums;

/// <summary>
/// Status of a pending site upload awaiting mapping confirmation.
/// </summary>
public enum PendingUploadStatus
{
    /// <summary>
    /// Upload received, awaiting mapping analysis.
    /// </summary>
    Processing,

    /// <summary>
    /// Mapping suggestions generated, awaiting admin review.
    /// </summary>
    AwaitingConfirmation,

    /// <summary>
    /// Mappings confirmed by admin.
    /// </summary>
    Confirmed,

    /// <summary>
    /// Upload failed or was rejected.
    /// </summary>
    Failed,

    /// <summary>
    /// Upload was cancelled/deleted.
    /// </summary>
    Cancelled
}
