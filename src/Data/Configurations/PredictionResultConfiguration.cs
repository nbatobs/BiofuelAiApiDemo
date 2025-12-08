using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class PredictionResultConfiguration : IEntityTypeConfiguration<PredictionResult>
{
    public void Configure(EntityTypeBuilder<PredictionResult> builder)
    {
        // Table and schema
        builder.ToTable("PredictionResults", "ml");

        // Properties
        builder.Property(p => p.ScenarioName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ScenarioDescription)
            .HasMaxLength(1000);

        builder.Property(p => p.ScenarioParametersJson)
            .HasColumnType("jsonb");

        builder.Property(p => p.InputDataJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(p => p.PredictionOutputJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(p => p.Site)
            .WithMany()
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(p => p.ModelVersion)
            .WithMany()
            .HasForeignKey(p => p.ModelVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(p => new { p.SiteId, p.CreatedAt });

        builder.HasIndex(p => p.UserId);

        builder.HasIndex(p => p.ModelVersionId);
    }
}
