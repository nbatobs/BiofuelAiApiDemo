using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class TrainingJobConfiguration : IEntityTypeConfiguration<TrainingJob>
{
    public void Configure(EntityTypeBuilder<TrainingJob> builder)
    {
        // Table and schema
        builder.ToTable("TrainingJobs", "ml");

        // Properties
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.ConfigJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(t => t.LogsJson)
            .HasColumnType("jsonb");

        builder.Property(t => t.ErrorMessage)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(t => t.Site)
            .WithMany()
            .HasForeignKey(t => t.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ModelVersion)
            .WithMany()
            .HasForeignKey(t => t.ModelVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(t => t.TriggeredBy)
            .WithMany()
            .HasForeignKey(t => t.TriggeredById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(t => new { t.SiteId, t.ScheduledAt });

        builder.HasIndex(t => new { t.Status, t.ScheduledAt });
    }
}
