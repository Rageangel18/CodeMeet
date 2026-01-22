using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("audit_log");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Action)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(e => e.SubjectType)
            .HasMaxLength(80);

        entity.Property(e => e.Ip)
            .HasMaxLength(45);

        entity.Property(e => e.UserAgent)
            .HasMaxLength(256);

        entity.HasOne(e => e.Org)
            .WithMany(o => o.AuditLogs)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.Session)
            .WithMany(s => s.AuditLogs)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.OrgId, e.CreatedUtc });
        entity.HasIndex(e => new { e.SessionId, e.CreatedUtc });
        entity.HasIndex(e => new { e.UserId, e.CreatedUtc });
    }
}
