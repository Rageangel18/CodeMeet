using CodeMeet.Contracts.Exec.Messages;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IExecRunPublisher
{
    Task PublishRunAsync(ExecRunRequestedMessage message, CancellationToken ct = default);
}
