using CodeMeet.DAL.Enums;

namespace CodeMeet.Contracts.Exec.Messages;

public sealed class ExecRunRequestedMessage
{
    public Guid ExecRequestId { get; init; }
    public CodeLanguage Language { get; init; } = CodeLanguage.CSharp;
    public string Code { get; init; } = string.Empty;
}
