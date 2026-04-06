using System.Text;

namespace ElectivesApp.Core;

// ── Simple template engine ────────────────────────────────────────────────────
// Processes {{key}} placeholders and {{#if condition}}...{{/if}} blocks
// and {{#each items}}...{{/each}} loops from a dictionary model.
public static class TemplateEngine
{
    /// <summary>Renders an HTML template file with the given model dictionary.</summary>
    public static string Render(string templatePath, Dictionary<string, object?> model)
    {
        var template = File.ReadAllText(templatePath);
        return RenderString(template, model);
    }

    /// <summary>Renders a template string with the given model dictionary.</summary>
    public static string RenderString(string template, Dictionary<string, object?> model)
    {
        var sb = new StringBuilder(template);

        // 1. Replace simple {{key}} placeholders
        foreach (var (key, value) in model)
        {
            if (value is not IEnumerable<object> && value is not bool)
                sb.Replace("{{" + key + "}}", Encode(value?.ToString() ?? ""));
        }

        // 2. Process {{#if key}} ... {{/if}}
        var result = ProcessIf(sb.ToString(), model);

        // 3. Process {{#each key}} ... {{/each}}
        result = ProcessEach(result, model);

        return result;
    }

    private static string ProcessIf(string template, Dictionary<string, object?> model)
    {
        return System.Text.RegularExpressions.Regex.Replace(template,
            @"\{\{#if (\w+)\}\}(.*?)\{\{/if\}\}",
            m =>
            {
                var key = m.Groups[1].Value;
                var body = m.Groups[2].Value;
                var val = model.GetValueOrDefault(key);
                bool truthy = val switch
                {
                    bool b => b,
                    string s => !string.IsNullOrEmpty(s),
                    null => false,
                    _ => true
                };
                return truthy ? body : "";
            },
            System.Text.RegularExpressions.RegexOptions.Singleline);
    }

    private static string ProcessEach(string template, Dictionary<string, object?> model)
    {
        return System.Text.RegularExpressions.Regex.Replace(template,
            @"\{\{#each (\w+)\}\}(.*?)\{\{/each\}\}",
            m =>
            {
                var key = m.Groups[1].Value;
                var body = m.Groups[2].Value;
                if (!model.TryGetValue(key, out var val) || val == null) return "";
                if (val is not System.Collections.IEnumerable items) return "";

                var sb = new StringBuilder();
                foreach (var item in items)
                {
                    var itemModel = item is Dictionary<string, object?> dict
                        ? dict
                        : ObjectToDictionary(item);
                    var rowModel = new Dictionary<string, object?>(model);
                    foreach (var (k, v) in itemModel) rowModel[k] = v;
                    sb.Append(RenderString(body, rowModel));
                }
                return sb.ToString();
            },
            System.Text.RegularExpressions.RegexOptions.Singleline);
    }

    private static string Encode(string s) =>
        System.Net.WebUtility.HtmlEncode(s);

    private static Dictionary<string, object?> ObjectToDictionary(object obj)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in obj.GetType().GetProperties())
            dict[prop.Name] = prop.GetValue(obj)?.ToString();
        return dict;
    }
}