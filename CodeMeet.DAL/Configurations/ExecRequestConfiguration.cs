using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class ExecRequestConfiguration : IEntityTypeConfiguration<ExecRequest>
{
    public void Configure(EntityTypeBuilder<ExecRequest> entity)
    {
        entity.ToTable("exec_requests");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Language)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(ExecStatus.Queued);

        entity.Property(e => e.Truncated)
            .HasDefaultValue(false);

        entity.HasOne(e => e.Session)
            .WithMany(s => s.ExecRequests)
            .HasForeignKey(e => e.SessionId);

        entity.HasOne(e => e.Snippet)
            .WithMany(s => s.ExecRequests)
            .HasForeignKey(e => e.SnippetId);

        entity.HasOne(e => e.RequestedByUser)
            .WithMany(u => u.ExecRequests)
            .HasForeignKey(e => e.RequestedByUserId);

        entity.HasIndex(e => new { e.SessionId, e.CreatedUtc });
        entity.HasIndex(e => new { e.SessionId, e.Status });
    }
}
