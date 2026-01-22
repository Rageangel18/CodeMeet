using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class OrganizationUserConfiguration : IEntityTypeConfiguration<OrganizationUser>
{
    public void Configure(EntityTypeBuilder<OrganizationUser> entity)
    {
        entity.ToTable("organization_users");

        entity.HasKey(e => new { e.OrgId, e.UserId });

        entity.Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.HasOne(e => e.Org)
            .WithMany(o => o.Members)
            .HasForeignKey(e => e.OrgId);

        entity.HasOne(e => e.User)
            .WithMany(u => u.OrganizationMemberships)
            .HasForeignKey(e => e.UserId);

        entity.HasIndex(e => new { e.OrgId, e.Role });
    }
}
