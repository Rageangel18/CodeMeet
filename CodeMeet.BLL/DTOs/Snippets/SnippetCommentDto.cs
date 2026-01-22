namespace CodeMeet.BLL.DTOs.Snippets;

public class SnippetCommentDto
{
    public Guid Id { get; set; }
    public Guid SnippetId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public int? LineStart { get; set; }
    public int? LineEnd { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
}
