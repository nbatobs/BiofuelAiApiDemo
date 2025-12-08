using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class DataCleaningRuleConfiguration : IEntityTypeConfiguration<DataCleaningRule>
{
    public void Configure(EntityTypeBuilder<DataCleaningRule> builder)
    {
        // Table and schema
        builder.ToTable("DataCleaningRules", "config");

        // Properties
        builder.Property(r => r.RuleType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.ConfigJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(r => r.VersionNumber)
            .IsRequired()
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(r => r.Site)
            .WithMany()
            .HasForeignKey(r => r.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(r => new { r.SiteId, r.IsActive, r.Priority });
    }
}
