using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class SiteDataSchemaConfiguration : IEntityTypeConfiguration<SiteDataSchema>
{
    public void Configure(EntityTypeBuilder<SiteDataSchema> builder)
    {
        // Table and schema
        builder.ToTable("SiteDataSchemas", "config");

        // Properties
        builder.Property(s => s.VersionNumber)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.SchemaDefinition)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(s => s.ChangeDescription)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(s => s.Site)
            .WithMany()
            .HasForeignKey(s => s.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(s => new { s.SiteId, s.VersionNumber });
    }
}
