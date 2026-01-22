using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> entity)
    {
        entity.ToTable("sessions");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(SessionStatus.Draft);

        entity.Property(e => e.DefaultLanguage)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(CodeLanguage.CSharp);

        entity.Property(e => e.IsExecEnabled)
            .HasDefaultValue(true);

        entity.Property(e => e.RecordingBlobUrl)
            .HasMaxLength(500);

        entity.HasOne(e => e.Org)
            .WithMany(o => o.Sessions)
            .HasForeignKey(e => e.OrgId);

        entity.HasOne(e => e.CreatedByUser)
            .WithMany(u => u.CreatedSessions)
            .HasForeignKey(e => e.CreatedByUserId);

        entity.HasIndex(e => new { e.OrgId, e.Status });
        entity.HasIndex(e => e.ScheduledStartUtc);
    }
}
