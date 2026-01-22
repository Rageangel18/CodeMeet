using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class SnippetConfiguration : IEntityTypeConfiguration<Snippet>
{
    public void Configure(EntityTypeBuilder<Snippet> entity)
    {
        entity.ToTable("snippets");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Filename)
            .HasMaxLength(120);

        entity.Property(e => e.Language)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(CodeLanguage.CSharp);

        entity.Property(e => e.IsFinalSolution)
            .HasDefaultValue(false);

        entity.HasOne(e => e.Session)
            .WithMany(s => s.Snippets)
            .HasForeignKey(e => e.SessionId);

        entity.HasOne(e => e.AuthorUser)
            .WithMany(u => u.Snippets)
            .HasForeignKey(e => e.AuthorUserId);

        entity.HasIndex(e => new { e.SessionId, e.CreatedUtc });
        entity.HasIndex(e => new { e.SessionId, e.IsFinalSolution });
    }
}
