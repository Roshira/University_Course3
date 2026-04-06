using ElectivesApp.Core;
using ElectivesApp.Infrastructure;
using Microsoft.Extensions.Logging;

// ───── Logging setup ─────
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole()
        .AddProvider(new FileLoggerProvider("logs/app.log"));
});

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Starting Electives application...");

// ───── Config ─────
var config = AppConfig.Load("appsettings.json");

// ───── Front Controller HTTP server ─────
var server = new FrontController(config, loggerFactory);
await server.StartAsync();