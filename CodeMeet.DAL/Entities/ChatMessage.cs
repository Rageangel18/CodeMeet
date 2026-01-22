public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }

    public Session Session { get; set; } = null!;
    public User? AuthorUser { get; set; }
}
