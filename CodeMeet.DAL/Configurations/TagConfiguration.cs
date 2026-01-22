using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> entity)
    {
        entity.ToTable("tags");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .HasMaxLength(80)
            .IsRequired();

        entity.HasOne(e => e.Org)
            .WithMany(o => o.Tags)
            .HasForeignKey(e => e.OrgId);

        entity.HasIndex(e => new { e.OrgId, e.Name })
            .IsUnique();
    }
}
