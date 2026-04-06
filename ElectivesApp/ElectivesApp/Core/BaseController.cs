using System.Net;
using System.Text;
using System.Web;

namespace ElectivesApp.Core;

// ── Base Controller ───────────────────────────────────────────────────────────
public abstract class BaseController
{
    protected readonly string ViewsPath;

    protected BaseController(string viewsPath = "Views")
    {
        ViewsPath = viewsPath;
    }

    // ── Render HTML from template ─────────────────────────────────────────────
    protected async Task View(HttpContext ctx, string template,
        Dictionary<string, object?> model, int statusCode = 200)
    {
        var path = Path.Combine(ViewsPath, template);
        var html = TemplateEngine.Render(path, model);
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "text/html; charset=utf-8";
        var bytes = Encoding.UTF8.GetBytes(html);
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
        ctx.Response.OutputStream.Close();
    }

    // ── Redirect ──────────────────────────────────────────────────────────────
    protected void Redirect(HttpContext ctx, string url)
    {
        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = url;
        ctx.Response.OutputStream.Close();
    }

    // ── Parse form body ───────────────────────────────────────────────────────
    protected static async Task<Dictionary<string, string>> ReadFormAsync(HttpListenerRequest req)
    {
        using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        return ParseQueryString(body);
    }

    protected static Dictionary<string, string> ParseQueryString(string qs)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in qs.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx < 0) continue;
            var key = HttpUtility.UrlDecode(pair[..idx]);
            var val = HttpUtility.UrlDecode(pair[(idx + 1)..]);
            dict[key] = val;
        }
        return dict;
    }

    // ── Send JSON ─────────────────────────────────────────────────────────────
    protected async Task Json(HttpContext ctx, string json, int status = 200)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        var bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
        ctx.Response.OutputStream.Close();
    }
}