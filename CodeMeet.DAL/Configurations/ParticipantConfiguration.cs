using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> entity)
    {
        entity.ToTable("participants");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(e => e.Email)
            .HasMaxLength(320);

        entity.Property(e => e.Status)
            .HasMaxLength(40);

        entity.Property(e => e.IsGuest)
            .HasDefaultValue(false);

        entity.HasOne(e => e.Session)
            .WithMany(s => s.Participants)
            .HasForeignKey(e => e.SessionId);

        entity.HasOne(e => e.User)
            .WithMany(u => u.Participants)
            .HasForeignKey(e => e.UserId);

        entity.HasIndex(e => new { e.SessionId, e.Role });
        entity.HasIndex(e => new { e.SessionId, e.CreatedUtc });
        entity.HasIndex(e => e.Email);
    }
}
