namespace CodeMeet.Hubs;

public static class SessionHubGroups
{
    public static string SessionGroup(string sessionId) => $"session-{sessionId}";
}
