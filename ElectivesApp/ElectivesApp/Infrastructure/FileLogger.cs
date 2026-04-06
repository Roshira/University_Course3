using Microsoft.Extensions.Logging;

namespace ElectivesApp.Infrastructure;

// ── File logger provider ──────────────────────────────────────────────────────
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _path;
    private readonly StreamWriter _writer;
    private readonly object _lock = new();

    public FileLoggerProvider(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        _writer = new StreamWriter(path, append: true) { AutoFlush = true };
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, _writer, _lock);

    public void Dispose() => _writer.Dispose();
}

// ── File logger ───────────────────────────────────────────────────────────────
public sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly StreamWriter _writer;
    private readonly object _lock;

    public FileLogger(string category, StreamWriter writer, object @lock)
    {
        _category = category;
        _writer = writer;
        _lock = @lock;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel level) => level >= LogLevel.Information;

    public void Log<TState>(LogLevel level, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(level)) return;
        var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level,-11}] [{_category}] {formatter(state, exception)}";
        if (exception != null) msg += $"\n{exception}";
        lock (_lock) { _writer.WriteLine(msg); }
    }
}