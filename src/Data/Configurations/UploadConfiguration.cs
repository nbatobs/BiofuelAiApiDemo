using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class UploadConfiguration : IEntityTypeConfiguration<Upload>
{
    public void Configure(EntityTypeBuilder<Upload> builder)
    {
        // Table and schema
        builder.ToTable("Uploads", "data");

        // Properties
        builder.Property(u => u.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.ValidationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.ErrorMessage)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(u => u.Site)
            .WithMany()
            .HasForeignKey(u => u.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(u => new { u.SiteId, u.UploadedAt });

        builder.HasIndex(u => u.UserId);
    }
}
