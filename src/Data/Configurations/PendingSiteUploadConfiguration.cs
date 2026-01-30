using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class PendingSiteUploadConfiguration : IEntityTypeConfiguration<PendingSiteUpload>
{
    public void Configure(EntityTypeBuilder<PendingSiteUpload> builder)
    {
        // Table and schema
        builder.ToTable("PendingSiteUploads", "data");

        // Properties
        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.BlobPath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.MappingResultJson)
            .HasColumnType("jsonb");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(p => p.UploadedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.Site)
            .WithMany()
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.UploadedBy)
            .WithMany()
            .HasForeignKey(p => p.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ConfirmedBy)
            .WithMany()
            .HasForeignKey(p => p.ConfirmedByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(p => p.ResultingSchemaVersion)
            .WithMany()
            .HasForeignKey(p => p.ResultingSchemaVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(p => new { p.SiteId, p.Status });
        builder.HasIndex(p => p.UploadedByUserId);
        builder.HasIndex(p => p.Status);
    }
}
