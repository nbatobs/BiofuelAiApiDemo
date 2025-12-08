using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class DataValidationRuleConfiguration : IEntityTypeConfiguration<DataValidationRule>
{
    public void Configure(EntityTypeBuilder<DataValidationRule> builder)
    {
        // Table and schema
        builder.ToTable("DataValidationRules", "config");

        // Properties
        builder.Property(r => r.ColumnName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.RuleType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.ConfigJson)
            .IsRequired()
            .HasColumnType("jsonb");

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
