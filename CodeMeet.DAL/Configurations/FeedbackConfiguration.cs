using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> entity)
    {
        entity.ToTable("feedback");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Recommendation)
            .HasConversion<string>()
            .HasMaxLength(20);


        entity.Property(e => e.ScoreOverall)
            .HasColumnType("decimal(3,1)");

        entity.Property(e => e.ScoreTech)
            .HasColumnType("decimal(3,1)");

        entity.Property(e => e.ScoreComm)
            .HasColumnType("decimal(3,1)");


        entity.HasOne(e => e.Session)
            .WithMany(s => s.Feedbacks)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);


        entity.HasOne(e => e.ReviewerUser)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(e => e.ReviewerUserId)
            .OnDelete(DeleteBehavior.NoAction);


        entity.HasOne(e => e.CandidateParticipant)
            .WithMany(p => p.FeedbacksAsCandidate)
            .HasForeignKey(e => e.CandidateParticipantId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasIndex(e => new { e.SessionId, e.CreatedUtc });
        entity.HasIndex(e => e.ReviewerUserId);
        entity.HasIndex(e => e.CandidateParticipantId);
    }
}
