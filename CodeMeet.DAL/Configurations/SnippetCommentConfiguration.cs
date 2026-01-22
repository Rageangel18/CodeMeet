using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class SnippetCommentConfiguration : IEntityTypeConfiguration<SnippetComment>
{
    public void Configure(EntityTypeBuilder<SnippetComment> entity)
    {
        entity.ToTable("snippet_comments");

        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Snippet)
            .WithMany(s => s.Comments)
            .HasForeignKey(e => e.SnippetId);

        entity.HasOne(e => e.AuthorUser)
            .WithMany(u => u.SnippetComments)
            .HasForeignKey(e => e.AuthorUserId);

        entity.HasIndex(e => new { e.SnippetId, e.CreatedUtc });
    }
}
