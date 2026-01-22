using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class QuestionTagConfiguration : IEntityTypeConfiguration<QuestionTag>
{
    public void Configure(EntityTypeBuilder<QuestionTag> entity)
    {
        entity.ToTable("question_tags");

        entity.HasKey(e => new { e.QuestionId, e.TagId });

        entity.HasOne(e => e.Question)
            .WithMany(q => q.QuestionTags)
            .HasForeignKey(e => e.QuestionId)
            .OnDelete(DeleteBehavior.Cascade); 

        entity.HasOne(e => e.Tag)
            .WithMany(t => t.QuestionTags)
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.NoAction); 

        entity.HasIndex(e => e.TagId);
    }
}
