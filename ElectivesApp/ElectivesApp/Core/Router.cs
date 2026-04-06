using System.Net;
using System.Text.RegularExpressions;

namespace ElectivesApp.Core;

// ── Route definition ──────────────────────────────────────────────────────────
public class RouteDefinition
{
    public string Method { get; }
    public Regex Pattern { get; }
    public string[] ParamNames { get; }
    public Func<HttpContext, Task> Handler { get; }
    public bool RequiresAuth { get; }
    public string? RequiredRole { get; }  // null = any role

    public RouteDefinition(string method, string pattern,
        Func<HttpContext, Task> handler,
        bool requiresAuth = true, string? requiredRole = null)
    {
        Method = method.ToUpper();
        Handler = handler;
        RequiresAuth = requiresAuth;
        RequiredRole = requiredRole;

        // Convert "/courses/{id}" → regex + param names
        var paramNames = new List<string>();
        var regexStr = "^" + Regex.Replace(pattern, @"\{(\w+)\}", m =>
        {
            paramNames.Add(m.Groups[1].Value);
            return @"([^/]+)";
        }) + "$";
        Pattern = new Regex(regexStr, RegexOptions.IgnoreCase);
        ParamNames = paramNames.ToArray();
    }
}

// ── HTTP context (wraps request + response + route params + session) ──────────
public class HttpContext
{
    public HttpListenerRequest Request { get; }
    public HttpListenerResponse Response { get; }
    public Dictionary<string, string> RouteParams { get; } = new();
    public Session? Session { get; set; }

    public HttpContext(HttpListenerRequest request, HttpListenerResponse response)
    {
        Request = request;
        Response = response;
    }

    public int GetRouteInt(string name) =>
        int.TryParse(RouteParams.GetValueOrDefault(name), out var v) ? v : 0;
}

// ── Router ────────────────────────────────────────────────────────────────────
public class Router
{
    private readonly List<RouteDefinition> _routes = new();

    public void Get(string pattern, Func<HttpContext, Task> handler,
        bool requiresAuth = true, string? role = null)
        => _routes.Add(new RouteDefinition("GET", pattern, handler, requiresAuth, role));

    public void Post(string pattern, Func<HttpContext, Task> handler,
        bool requiresAuth = true, string? role = null)
        => _routes.Add(new RouteDefinition("POST", pattern, handler, requiresAuth, role));

    public (RouteDefinition? route, Dictionary<string, string> @params) Match(
        string method, string path)
    {
        foreach (var route in _routes)
        {
            if (route.Method != method.ToUpper()) continue;
            var m = route.Pattern.Match(path);
            if (!m.Success) continue;

            var p = new Dictionary<string, string>();
            for (int i = 0; i < route.ParamNames.Length; i++)
                p[route.ParamNames[i]] = m.Groups[i + 1].Value;
            return (route, p);
        }
        return (null, new());
    }
}