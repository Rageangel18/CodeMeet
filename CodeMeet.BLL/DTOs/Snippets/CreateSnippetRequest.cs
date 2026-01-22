using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Snippets;

public class CreateSnippetRequest
{
    public Guid SessionId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public CodeLanguage? Language { get; set; }
    public string? Filename { get; set; }
    public string Content { get; set; } = null!;
    public bool MarkAsFinal { get; set; }
}
