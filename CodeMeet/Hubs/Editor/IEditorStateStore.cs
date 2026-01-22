namespace CodeMeet.Hubs.Editor;

public interface IEditorStateStore
{
    Task<EditorState?> GetAsync(string sessionId, CancellationToken ct);
    Task UpsertAsync(EditorState state, CancellationToken ct);
}
