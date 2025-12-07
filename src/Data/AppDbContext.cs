using Microsoft.EntityFrameworkCore;
using Data.Models;
using Data.Models.Enums;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Site> Sites { get; set; }
    public DbSet<SiteDataSchema> SiteDataSchemas { get; set; }
    public DbSet<UserSiteAccess> UserSiteAccesses { get; set; }
    public DbSet<DataCleaningRule> DataCleaningRules { get; set; }
    public DbSet<DataValidationRule> DataValidationRules { get; set; }
    public DbSet<DataRow> DataRows { get; set; }
    public DbSet<Upload> Uploads { get; set; }
    public DbSet<ModelVersion> ModelVersions { get; set; }
    public DbSet<PredictionResult> PredictionResults { get; set; }
    public DbSet<InferenceRequest> InferenceRequests { get; set; }
    public DbSet<TrainingJob> TrainingJobs { get; set; }
    public DbSet<Dashboard> Dashboards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Schema Organization =====
        // Core domain entities
        modelBuilder.Entity<Company>().ToTable("Companies", "core");
        modelBuilder.Entity<User>().ToTable("Users", "core");
        modelBuilder.Entity<Site>().ToTable("Sites", "core");
        modelBuilder.Entity<UserSiteAccess>().ToTable("UserSiteAccess", "core");

        // Configuration entities
        modelBuilder.Entity<SiteDataSchema>().ToTable("SiteDataSchemas", "config");
        modelBuilder.Entity<DataCleaningRule>().ToTable("DataCleaningRules", "config");
        modelBuilder.Entity<DataValidationRule>().ToTable("DataValidationRules", "config");
        modelBuilder.Entity<Dashboard>().ToTable("Dashboards", "config");

        // Data storage entities
        modelBuilder.Entity<DataRow>().ToTable("DataRows", "data");
        modelBuilder.Entity<Upload>().ToTable("Uploads", "data");

        // ML/AI entities
        modelBuilder.Entity<ModelVersion>().ToTable("ModelVersions", "ml");
        modelBuilder.Entity<TrainingJob>().ToTable("TrainingJobs", "ml");
        modelBuilder.Entity<InferenceRequest>().ToTable("InferenceRequests", "ml");
        modelBuilder.Entity<PredictionResult>().ToTable("PredictionResults", "ml");

        // Composite key for UserSiteAccess
        modelBuilder.Entity<UserSiteAccess>()
            .HasKey(u => new { u.UserId, u.SiteId });

        // ===== Relationship Configurations =====
        
        // User -> Company (optional for individual users/consultants)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Site -> Company
        modelBuilder.Entity<Site>()
            .HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Site -> CurrentSchemaVersion (optional self-referential FK)
        modelBuilder.Entity<Site>()
            .HasOne<SiteDataSchema>()
            .WithMany()
            .HasForeignKey(s => s.CurrentSchemaVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // SiteDataSchema -> Site
        modelBuilder.Entity<SiteDataSchema>()
            .HasOne(s => s.Site)
            .WithMany()
            .HasForeignKey(s => s.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // SiteDataSchema -> CreatedBy (User)
        modelBuilder.Entity<SiteDataSchema>()
            .HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // DataRow -> Site
        modelBuilder.Entity<DataRow>()
            .HasOne(d => d.Site)
            .WithMany()
            .HasForeignKey(d => d.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // DataRow -> SchemaVersion
        modelBuilder.Entity<DataRow>()
            .HasOne(d => d.SchemaVersion)
            .WithMany()
            .HasForeignKey(d => d.SchemaVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // DataCleaningRule -> Site
        modelBuilder.Entity<DataCleaningRule>()
            .HasOne(r => r.Site)
            .WithMany()
            .HasForeignKey(r => r.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // DataCleaningRule -> CreatedBy (User) - nullable
        modelBuilder.Entity<DataCleaningRule>()
            .HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Upload -> Site
        modelBuilder.Entity<Upload>()
            .HasOne(u => u.Site)
            .WithMany()
            .HasForeignKey(u => u.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Upload -> User - nullable
        modelBuilder.Entity<Upload>()
            .HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // ModelVersion -> Site
        modelBuilder.Entity<ModelVersion>()
            .HasOne(m => m.Site)
            .WithMany()
            .HasForeignKey(m => m.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // PredictionResult -> Site
        modelBuilder.Entity<PredictionResult>()
            .HasOne(p => p.Site)
            .WithMany()
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // PredictionResult -> User - nullable
        modelBuilder.Entity<PredictionResult>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // PredictionResult -> ModelVersion - nullable
        modelBuilder.Entity<PredictionResult>()
            .HasOne(p => p.ModelVersion)
            .WithMany()
            .HasForeignKey(p => p.ModelVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // UserSiteAccess -> User
        modelBuilder.Entity<UserSiteAccess>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserSiteAccess -> Site
        modelBuilder.Entity<UserSiteAccess>()
            .HasOne(a => a.Site)
            .WithMany()
            .HasForeignKey(a => a.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // DataValidationRule -> Site
        modelBuilder.Entity<DataValidationRule>()
            .HasOne(r => r.Site)
            .WithMany()
            .HasForeignKey(r => r.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // DataValidationRule -> CreatedBy (User) - nullable
        modelBuilder.Entity<DataValidationRule>()
            .HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // InferenceRequest -> Site
        modelBuilder.Entity<InferenceRequest>()
            .HasOne(i => i.Site)
            .WithMany()
            .HasForeignKey(i => i.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // InferenceRequest -> User - nullable
        modelBuilder.Entity<InferenceRequest>()
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // InferenceRequest -> PredictionResult - nullable
        modelBuilder.Entity<InferenceRequest>()
            .HasOne(i => i.PredictionResult)
            .WithMany()
            .HasForeignKey(i => i.PredictionResultId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // TrainingJob -> Site
        modelBuilder.Entity<TrainingJob>()
            .HasOne(t => t.Site)
            .WithMany()
            .HasForeignKey(t => t.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // TrainingJob -> ModelVersion - nullable
        modelBuilder.Entity<TrainingJob>()
            .HasOne(t => t.ModelVersion)
            .WithMany()
            .HasForeignKey(t => t.ModelVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // TrainingJob -> TriggeredBy (User) - nullable
        modelBuilder.Entity<TrainingJob>()
            .HasOne(t => t.TriggeredBy)
            .WithMany()
            .HasForeignKey(t => t.TriggeredById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Dashboard -> Site
        modelBuilder.Entity<Dashboard>()
            .HasOne(d => d.Site)
            .WithMany()
            .HasForeignKey(d => d.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Dashboard -> CreatedBy (User)
        modelBuilder.Entity<Dashboard>()
            .HasOne(d => d.CreatedBy)
            .WithMany()
            .HasForeignKey(d => d.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Enum to String Conversions =====
        
        modelBuilder.Entity<PredictionResult>()
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Upload>()
            .Property(u => u.ValidationStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<UserSiteAccess>()
            .Property(a => a.RoleOnSite)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<DataCleaningRule>()
            .Property(r => r.RuleType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<DataValidationRule>()
            .Property(r => r.RuleType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<InferenceRequest>()
            .Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<TrainingJob>()
            .Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // ===== Indexes for Performance =====
        
        // User indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.CompanyId);

        // Site indexes
        modelBuilder.Entity<Site>()
            .HasIndex(s => s.CompanyId);
        
        modelBuilder.Entity<Site>()
            .HasIndex(s => s.CurrentSchemaVersionId);

        // SiteDataSchema indexes
        modelBuilder.Entity<SiteDataSchema>()
            .HasIndex(s => new { s.SiteId, s.VersionNumber });

        // DataRow indexes (critical for time-series queries)
        modelBuilder.Entity<DataRow>()
            .HasIndex(d => new { d.SiteId, d.Date });
        
        modelBuilder.Entity<DataRow>()
            .HasIndex(d => d.SchemaVersionId);

        // DataCleaningRule indexes
        modelBuilder.Entity<DataCleaningRule>()
            .HasIndex(r => new { r.SiteId, r.IsActive, r.Priority });

        // Upload indexes
        modelBuilder.Entity<Upload>()
            .HasIndex(u => new { u.SiteId, u.UploadedAt });
        
        modelBuilder.Entity<Upload>()
            .HasIndex(u => u.UserId);

        // ModelVersion indexes
        modelBuilder.Entity<ModelVersion>()
            .HasIndex(m => new { m.SiteId, m.IsActive });
        
        modelBuilder.Entity<ModelVersion>()
            .HasIndex(m => new { m.SiteId, m.VersionNumber });

        // PredictionResult indexes
        modelBuilder.Entity<PredictionResult>()
            .HasIndex(p => new { p.SiteId, p.CreatedAt });
        
        modelBuilder.Entity<PredictionResult>()
            .HasIndex(p => p.UserId);
        
        modelBuilder.Entity<PredictionResult>()
            .HasIndex(p => p.ModelVersionId);

        // DataValidationRule indexes
        modelBuilder.Entity<DataValidationRule>()
            .HasIndex(r => new { r.SiteId, r.IsActive, r.Priority });

        // InferenceRequest indexes
        modelBuilder.Entity<InferenceRequest>()
            .HasIndex(i => new { i.SiteId, i.RequestedAt });
        
        modelBuilder.Entity<InferenceRequest>()
            .HasIndex(i => new { i.Status, i.RequestedAt });
        
        modelBuilder.Entity<InferenceRequest>()
            .HasIndex(i => i.UserId);

        // TrainingJob indexes
        modelBuilder.Entity<TrainingJob>()
            .HasIndex(t => new { t.SiteId, t.ScheduledAt });
        
        modelBuilder.Entity<TrainingJob>()
            .HasIndex(t => new { t.Status, t.ScheduledAt });

        // Dashboard indexes
        modelBuilder.Entity<Dashboard>()
            .HasIndex(d => new { d.SiteId, d.IsPublic });
        
        modelBuilder.Entity<Dashboard>()
            .HasIndex(d => d.CreatedById);

        // Keep JSON fields as strings — provider-specific mapping handled by provider
    }
}
