using CodeMeet.BLL.DTOs.Exec;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.Hubs.Editor;
using Microsoft.AspNetCore.SignalR;

namespace CodeMeet.Hubs;

public sealed class SessionHub : Hub
{
    private readonly ILogger<SessionHub> _log;
    private readonly IEditorStateStore _editorState;
    private readonly IExecOrchestrator _exec;

    public SessionHub(
        ILogger<SessionHub> log,
        IEditorStateStore editorState,
        IExecOrchestrator exec)
    {
        _log = log;
        _editorState = editorState;
        _exec = exec;
    }

    public async Task JoinSession(string sessionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("sessionId is empty");

            var group = SessionHubGroups.SessionGroup(sessionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            // Editor state on join
            var state = await _editorState.GetAsync(sessionId, Context.ConnectionAborted);
            if (state is not null)
            {
                await Clients.Caller.SendAsync(
                    "EditorStateReceived",
                    state.Language,
                    state.Code,
                    state.Version,
                    state.UpdatedBy,
                    state.UpdatedUtc);
            }

            _log.LogInformation("JoinSession ok. sessionId={SessionId} conn={ConnId}", sessionId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "JoinSession failed. sessionId={SessionId} conn={ConnId}", sessionId, Context.ConnectionId);
            throw new HubException($"JoinSession failed: {ex.Message}");
        }
    }

    public async Task LeaveSession(string sessionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return;

            var group = SessionHubGroups.SessionGroup(sessionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

            _log.LogInformation("LeaveSession ok. sessionId={SessionId} conn={ConnId}", sessionId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "LeaveSession failed. sessionId={SessionId} conn={ConnId}", sessionId, Context.ConnectionId);
            throw new HubException($"LeaveSession failed: {ex.Message}");
        }
    }

    // Video for sessionVideoHub.js
    public async Task SendVideoFrame(string sessionId, string dataUrl)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(dataUrl))
            return;

        await Clients.OthersInGroup(SessionHubGroups.SessionGroup(sessionId))
            .SendAsync("VideoFrameReceived", dataUrl);
    }

    // Audio for sessionVideoHub.js
    public async Task SendAudioChunk(string sessionId, string dataUrl)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(dataUrl))
            return;

        await Clients.OthersInGroup(SessionHubGroups.SessionGroup(sessionId))
            .SendAsync("AudioChunkReceived", dataUrl);
    }

    // Monaco sync (full snapshot)
    public async Task UpdateCode(string sessionId, string? userName, string language, string code, long version)
    {
        var state = new EditorState
        {
            SessionId = sessionId,
            Language = string.IsNullOrWhiteSpace(language) ? "CSharp" : language,
            Code = code ?? string.Empty,
            Version = version,
            UpdatedBy = NormalizeName(userName),
            UpdatedUtc = DateTime.UtcNow
        };

        await _editorState.UpsertAsync(state, Context.ConnectionAborted);

        await Clients.OthersInGroup(SessionHubGroups.SessionGroup(sessionId))
            .SendAsync("CodeUpdated", state.Language, state.Code, state.Version, state.UpdatedBy, state.UpdatedUtc);
    }

    public async Task UpdateCursor(string sessionId, string? userName, string cursorPayloadJson)
    {
        if (string.IsNullOrWhiteSpace(cursorPayloadJson))
            return;

        await Clients.OthersInGroup(SessionHubGroups.SessionGroup(sessionId))
            .SendAsync("CursorUpdated", NormalizeName(userName), cursorPayloadJson, DateTime.UtcNow);
    }

    // Run request
    public async Task RequestRun(string sessionId, string? userName, string language, string code, Guid? snippetId = null)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
            throw new HubException("Invalid sessionId");

        var created = await _exec.EnqueueAsync(new CreateExecRequestRequest
        {
            SessionId = sessionGuid,
            SnippetId = snippetId,
            RequestedByUserId = null,
            Language = MapLanguage(language),
            Code = code ?? string.Empty
        }, Context.ConnectionAborted);

        await Clients.Group(SessionHubGroups.SessionGroup(sessionId))
            .SendAsync("RunRequested", new
            {
                execRequestId = created.Id,
                requestedBy = NormalizeName(userName),
                createdUtc = created.CreatedUtc
            });
    }

    private static CodeMeet.DAL.Enums.CodeLanguage MapLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return CodeMeet.DAL.Enums.CodeLanguage.CSharp;

        var v = language.Trim();

        return v switch
        {
            "C#" => CodeMeet.DAL.Enums.CodeLanguage.CSharp,
            "CSharp" => CodeMeet.DAL.Enums.CodeLanguage.CSharp,
            "JavaScript" => CodeMeet.DAL.Enums.CodeLanguage.Javascript,
            "Javascript" => CodeMeet.DAL.Enums.CodeLanguage.Javascript,
            _ => CodeMeet.DAL.Enums.CodeLanguage.CSharp
        };
    }

    private static string NormalizeName(string? userName)
        => string.IsNullOrWhiteSpace(userName) ? "Guest" : userName.Trim();
}
