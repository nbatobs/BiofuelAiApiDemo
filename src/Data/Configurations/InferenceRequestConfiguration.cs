using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class InferenceRequestConfiguration : IEntityTypeConfiguration<InferenceRequest>
{
    public void Configure(EntityTypeBuilder<InferenceRequest> builder)
    {
        // Table and schema
        builder.ToTable("InferenceRequests", "ml");

        // Properties
        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.InputDataJson)
            .IsRequired()
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(i => i.Site)
            .WithMany()
            .HasForeignKey(i => i.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(i => i.PredictionResult)
            .WithMany()
            .HasForeignKey(i => i.PredictionResultId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(i => new { i.SiteId, i.RequestedAt });

        builder.HasIndex(i => new { i.Status, i.RequestedAt });

        builder.HasIndex(i => i.UserId);
    }
}
