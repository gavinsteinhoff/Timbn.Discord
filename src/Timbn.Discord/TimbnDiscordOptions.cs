namespace Timbn.Discord;

public class TimbnDiscordOptions
{
    public string? DiscordBotToken { get; set; } = null;
    public Func<LogMessage, Task>? Logger { get; set; }
    public ulong? DevDiscordGuidId { get; set; }
}

public class TimbnDiscordSettings
{
    public string? DiscordBotToken { get; set; } = null;
    public ulong? DevDiscordGuidId { get; set; }
}
