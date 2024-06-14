using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using Timbn.Discord.Interactions;

namespace Timbn.Discord;

internal class TimbnDiscord
{
    private readonly ILogger<TimbnDiscord> _logger;
    private readonly TimbnDiscordOptions _options;
    private readonly DiscordSocketClient _client;
    private readonly ITimbnInteractionHandler _handler;

    public TimbnDiscord(ILogger<TimbnDiscord> logger, TimbnDiscordOptions options, DiscordSocketClient client, ITimbnInteractionHandler handler)
    {
        _logger = logger;
        _options = options;
        _client = client;
        _handler = handler;
    }

    internal async Task Init()
    {
        if (_options.Logger is null)
        {
            _client.Log += LogAsync;
            _handler.AddLogger(LogAsync);
        }
        else
        {
            _client.Log += _options.Logger;
            _handler.AddLogger(_options.Logger);
        }

        var botToken = GetDiscordBotToken(_options);
        await _handler.InitializeAsync(_options);
        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();
    }

    private static string? GetDiscordBotToken(TimbnDiscordOptions options)
    {
        if (options.DiscordBotToken is not null)
            return options.DiscordBotToken;

        throw new ArgumentNullException("DiscordBotToken");
    }

    private Task LogAsync(LogMessage message)
    {
        _logger.Log(MapLogLevel(message.Severity), message.Exception, message.Message);
        return Task.CompletedTask;
    }

    private LogLevel MapLogLevel(LogSeverity level) => level switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose => LogLevel.Trace,
        LogSeverity.Debug => LogLevel.Debug,
        _ => LogLevel.Debug,
    };
}
