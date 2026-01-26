namespace Data.Models.Enums;

public enum SiteStatus
{
    PendingSetup,       // Created, awaiting initial data upload
    DataUploaded,       // Initial data received, awaiting schema mapping
    SchemaConfigured,   // Header mapping complete, ready for training
    ModelTraining,      // Training in progress
    Active,             // Live and operational
    Suspended           // Disabled (payment issue, maintenance, etc.)
}
