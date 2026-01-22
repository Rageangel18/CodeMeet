using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> entity)
    {
        entity.ToTable("invitations");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Email)
            .HasMaxLength(320)
            .IsRequired();

        entity.Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(e => e.Token)
            .HasMaxLength(100)
            .IsRequired();

        entity.HasOne(e => e.Session)
            .WithMany(s => s.Invitations)
            .HasForeignKey(e => e.SessionId);

        entity.HasOne(e => e.AcceptedUser)
            .WithMany(u => u.AcceptedInvitations)
            .HasForeignKey(e => e.AcceptedUserId);

        entity.HasIndex(e => new { e.SessionId, e.Email })
            .IsUnique();
    }
}
