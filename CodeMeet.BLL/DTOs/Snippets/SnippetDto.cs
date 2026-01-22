using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Snippets;

public class SnippetDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public CodeLanguage Language { get; set; }
    public string? Filename { get; set; }
    public string Content { get; set; } = null!;
    public bool IsFinalSolution { get; set; }
    public DateTime CreatedUtc { get; set; }
}
