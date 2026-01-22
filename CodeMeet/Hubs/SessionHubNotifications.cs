using CodeMeet.BLL.DTOs.Exec;
using Microsoft.AspNetCore.SignalR;

namespace CodeMeet.Hubs;

public static class SessionHubNotifications
{
    public static Task NotifyRunRequestedAsync(
        this IHubContext<SessionHub> hub,
        Guid sessionId,
        Guid execRequestId,
        string requestedBy,
        DateTime createdUtc,
        CancellationToken ct = default)
    {
        return hub.Clients
            .Group(SessionHubGroups.SessionGroup(sessionId.ToString()))
            .SendAsync("RunRequested", new
            {
                execRequestId,
                requestedBy,
                createdUtc
            }, ct);
    }

    public static Task NotifyRunCompletedAsync(
        this IHubContext<SessionHub> hub,
        Guid sessionId,
        ExecRequestDto result,
        CancellationToken ct = default)
    {
        return hub.Clients
            .Group(SessionHubGroups.SessionGroup(sessionId.ToString()))
            .SendAsync("RunCompleted", result, ct);
    }

    public static Task NotifyRunFailedAsync(
        this IHubContext<SessionHub> hub,
        Guid sessionId,
        object errorPayload,
        CancellationToken ct = default)
    {
        return hub.Clients
            .Group(SessionHubGroups.SessionGroup(sessionId.ToString()))
            .SendAsync("RunFailed", errorPayload, ct);
    }
}
