using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class SessionQuestionConfiguration : IEntityTypeConfiguration<SessionQuestion>
{
    public void Configure(EntityTypeBuilder<SessionQuestion> entity)
    {
        entity.ToTable("session_questions");

        entity.HasKey(e => new { e.SessionId, e.QuestionId });

        entity.Property(e => e.OrderIndex)
            .HasDefaultValue(0);

        entity.HasOne(e => e.Session)
            .WithMany(s => s.SessionQuestions)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Question)
            .WithMany(q => q.SessionQuestions)
            .HasForeignKey(e => e.QuestionId)
            .OnDelete(DeleteBehavior.NoAction); 

        entity.HasIndex(e => new { e.SessionId, e.OrderIndex });
    }
}
