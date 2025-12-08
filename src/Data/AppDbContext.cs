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

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
