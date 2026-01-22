namespace CodeMeet.BLL.DTOs.Chat;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public string? AuthorDisplayName { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
}
