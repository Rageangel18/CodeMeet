namespace CodeMeet.Hubs.Editor;

public sealed class EditorState
{
    public string SessionId { get; set; } = string.Empty;

    public string Language { get; set; } = "CSharp";
    public string Code { get; set; } = string.Empty;

    public long Version { get; set; }

    public string UpdatedBy { get; set; } = "Guest";
    public DateTime UpdatedUtc { get; set; }
}
