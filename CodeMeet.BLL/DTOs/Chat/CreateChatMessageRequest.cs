namespace CodeMeet.BLL.DTOs.Chat;

public class CreateChatMessageRequest
{
    public Guid SessionId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public string Text { get; set; } = null!;
}
