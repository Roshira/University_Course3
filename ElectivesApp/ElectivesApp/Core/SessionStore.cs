using System.Net;

namespace ElectivesApp.Core;

// ── Session store ─────────────────────────────────────────────────────────────
public class SessionStore
{
    private readonly Dictionary<string, Session> _sessions = new();
    private readonly object _lock = new();
    private readonly int _timeoutMinutes;

    public SessionStore(int timeoutMinutes = 30) => _timeoutMinutes = timeoutMinutes;

    public Session Create(int userId, string role, string username)
    {
        var session = new Session
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Role = role,
            Username = username,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_timeoutMinutes)
        };
        lock (_lock) { _sessions[session.Id] = session; }
        return session;
    }

    public Session? Get(string sessionId)
    {
        lock (_lock)
        {
            if (!_sessions.TryGetValue(sessionId, out var s)) return null;
            if (s.ExpiresAt < DateTime.UtcNow) { _sessions.Remove(sessionId); return null; }
            s.ExpiresAt = DateTime.UtcNow.AddMinutes(_timeoutMinutes); // sliding
            return s;
        }
    }

    public void Remove(string sessionId)
    {
        lock (_lock) { _sessions.Remove(sessionId); }
    }

    public void PurgeExpired()
    {
        lock (_lock)
        {
            var expired = _sessions.Where(kv => kv.Value.ExpiresAt < DateTime.UtcNow)
                                   .Select(kv => kv.Key).ToList();
            foreach (var k in expired) _sessions.Remove(k);
        }
    }
}

public class Session
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

// ── Session helpers ───────────────────────────────────────────────────────────
public static class SessionHelper
{
    public static Session? GetSession(HttpListenerRequest request,
        SessionStore store, string cookieName)
    {
        var cookie = request.Cookies[cookieName];
        if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value)) return null;
        return store.Get(cookie.Value);
    }

    public static void SetSessionCookie(HttpListenerResponse response,
        Session session, string cookieName)
    {
        var cookie = new Cookie(cookieName, session.Id)
        {
            HttpOnly = true,
            Path = "/",
            Expires = session.ExpiresAt
        };
        response.SetCookie(cookie);
    }

    public static void ClearSessionCookie(HttpListenerResponse response, string cookieName)
    {
        var cookie = new Cookie(cookieName, "")
        {
            HttpOnly = true,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        response.SetCookie(cookie);
    }
}