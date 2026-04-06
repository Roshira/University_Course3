using System.Text.Json;

namespace ElectivesApp.Infrastructure;

public class AppConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5000;
    public string SessionCookieName { get; set; } = "ELECTIVES_SESSION";
    public int SessionTimeoutMinutes { get; set; } = 30;

    public static AppConfig Load(string path)
    {
        if (!File.Exists(path))
            return CreateDefault(path);

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppConfig>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new AppConfig();
    }

    private static AppConfig CreateDefault(string path)
    {
        var config = new AppConfig
        {
            ConnectionString = "Host=localhost;Port=5432;Database=electives_db;Username=postgres;Password=postgres",
            Host = "localhost",
            Port = 5000,
            SessionCookieName = "ELECTIVES_SESSION",
            SessionTimeoutMinutes = 30
        };
        File.WriteAllText(path, JsonSerializer.Serialize(config,
            new JsonSerializerOptions { WriteIndented = true }));
        return config;
    }
}