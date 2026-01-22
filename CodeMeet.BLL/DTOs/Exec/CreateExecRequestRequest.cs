using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Exec;

public sealed class CreateExecRequestRequest
{
    public Guid SessionId { get; set; }
    public Guid? SnippetId { get; set; }
    public Guid? RequestedByUserId { get; set; }

    public CodeLanguage Language { get; set; } = CodeLanguage.CSharp;
    public string Code { get; set; } = string.Empty;
}
