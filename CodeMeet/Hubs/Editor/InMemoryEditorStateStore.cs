using System.Collections.Concurrent;

namespace CodeMeet.Hubs.Editor;

public sealed class InMemoryEditorStateStore : IEditorStateStore
{
    private static readonly ConcurrentDictionary<string, EditorState> Store = new();

    public Task<EditorState?> GetAsync(string sessionId, CancellationToken ct)
    {
        Store.TryGetValue(sessionId, out var state);
        return Task.FromResult(state);
    }

    public Task UpsertAsync(EditorState state, CancellationToken ct)
    {
        Store.AddOrUpdate(state.SessionId, state, (_, _) => state);
        return Task.CompletedTask;
    }
}
