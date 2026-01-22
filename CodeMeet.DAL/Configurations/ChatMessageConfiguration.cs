using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> entity)
    {
        entity.ToTable("chat_messages");

        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Session)
            .WithMany(s => s.ChatMessages)
            .HasForeignKey(e => e.SessionId);

        entity.HasOne(e => e.AuthorUser)
            .WithMany(u => u.ChatMessages)
            .HasForeignKey(e => e.AuthorUserId);

        entity.HasIndex(e => new { e.SessionId, e.CreatedUtc });
    }
}
