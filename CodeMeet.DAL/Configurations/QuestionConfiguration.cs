using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> entity)
    {
        entity.ToTable("questions");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(e => e.Level)
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.HasOne(e => e.Org)
            .WithMany(o => o.Questions)
            .HasForeignKey(e => e.OrgId);

        entity.HasOne(e => e.CreatedByUser)
            .WithMany(u => u.Questions)
            .HasForeignKey(e => e.CreatedByUserId);

        entity.HasIndex(e => new { e.OrgId, e.Level });
        entity.HasIndex(e => new { e.OrgId, e.CreatedUtc });
    }
}
