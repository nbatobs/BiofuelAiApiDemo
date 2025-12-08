using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Configurations;

public class UserSiteAccessConfiguration : IEntityTypeConfiguration<UserSiteAccess>
{
    public void Configure(EntityTypeBuilder<UserSiteAccess> builder)
    {
        // Table and schema
        builder.ToTable("UserSiteAccess", "core");

        // Composite primary key
        builder.HasKey(u => new { u.UserId, u.SiteId });

        // Properties
        builder.Property(a => a.RoleOnSite)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Site)
            .WithMany()
            .HasForeignKey(a => a.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
