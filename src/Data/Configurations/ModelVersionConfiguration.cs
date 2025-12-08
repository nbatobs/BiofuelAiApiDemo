using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class ModelVersionConfiguration : IEntityTypeConfiguration<ModelVersion>
{
    public void Configure(EntityTypeBuilder<ModelVersion> builder)
    {
        // Table and schema
        builder.ToTable("ModelVersions", "ml");

        // Properties
        builder.Property(m => m.BlobStoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.ModelFormat)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.ModelFramework)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.MetricsJson)
            .HasColumnType("jsonb");

        builder.Property(m => m.VersionNumber)
            .IsRequired()
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(m => m.Site)
            .WithMany()
            .HasForeignKey(m => m.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(m => new { m.SiteId, m.IsActive });

        builder.HasIndex(m => new { m.SiteId, m.VersionNumber });
    }
}
