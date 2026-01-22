using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Exec;

public sealed class CompleteExecRequestRequest
{
    public Guid Id { get; set; }

    public ExecStatus Status { get; set; } = ExecStatus.Completed;

    public int? ExitCode { get; set; }
    public string? Stdout { get; set; }
    public string? Stderr { get; set; }
    public int? DurationMs { get; set; }
    public bool Truncated { get; set; }
}
