namespace CodeMeet.Contracts.Exec.Messages;

public sealed class ExecRunCompletedMessage
{
    public Guid ExecRequestId { get; init; }

    public ExecRunResultStatus Status { get; init; }

    public int? ExitCode { get; init; }
    public string? Stdout { get; init; }
    public string? Stderr { get; init; }
    public int? DurationMs { get; init; }
    public bool Truncated { get; init; }
}
