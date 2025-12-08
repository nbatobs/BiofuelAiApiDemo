using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        // Table and schema
        builder.ToTable("Sites", "core");

        // Properties
        builder.Property(s => s.SiteName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.TimeZone)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ConfigJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(s => s.PowerBiWorkspaceId)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SiteDataSchema>()
            .WithMany()
            .HasForeignKey(s => s.CurrentSchemaVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(s => s.CompanyId);

        builder.HasIndex(s => s.CurrentSchemaVersionId);
    }
}
