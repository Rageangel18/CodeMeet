using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class ExportConfiguration : IEntityTypeConfiguration<Export>
{
    public void Configure(EntityTypeBuilder<Export> entity)
    {
        entity.ToTable("exports");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Type)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(e => e.BlobUrl)
            .HasMaxLength(500)
            .IsRequired();

        entity.HasOne(e => e.Org)
            .WithMany(o => o.Exports)
            .HasForeignKey(e => e.OrgId);

        entity.HasOne(e => e.Session)
            .WithMany(s => s.Exports)
            .HasForeignKey(e => e.SessionId);

        entity.HasOne(e => e.CreatedByUser)
            .WithMany(u => u.Exports)
            .HasForeignKey(e => e.CreatedByUserId);

        entity.HasIndex(e => new { e.OrgId, e.CreatedUtc });
        entity.HasIndex(e => new { e.SessionId, e.CreatedUtc });
    }
}
