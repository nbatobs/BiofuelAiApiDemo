using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class DataRowConfiguration : IEntityTypeConfiguration<DataRow>
{
    public void Configure(EntityTypeBuilder<DataRow> builder)
    {
        // Table and schema
        builder.ToTable("DataRows", "data");

        // Properties
        builder.Property(d => d.DataSourcesJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(d => d.SensorDataJson)
            .IsRequired()
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(d => d.Site)
            .WithMany()
            .HasForeignKey(d => d.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.SchemaVersion)
            .WithMany()
            .HasForeignKey(d => d.SchemaVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes (critical for time-series queries)
        builder.HasIndex(d => new { d.SiteId, d.Date });

        builder.HasIndex(d => d.SchemaVersionId);
    }
}
