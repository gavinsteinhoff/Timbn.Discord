using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Timbn.Discord.Interactions;

namespace Timbn.Discord;

public static class TimbnDiscordExtensions
{
    public static IServiceCollection AddTimbnDiscord<T>(this IServiceCollection services, Action<TimbnDiscordOptions>? optionsAction = null) 
        where T : TimbnInteractionHandler
    {
        var options = new TimbnDiscordOptions();
        optionsAction?.Invoke(options);
        
        return services
            .AddSingleton<TimbnDiscordOptions>(options)
            .AddSingleton<TimbnDiscord>()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<ITimbnInteractionHandler, T>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
    }

    public static async Task<WebApplication> RunTimbnDiscordAsync(this WebApplication app)
    {
        var timbnDiscord = app.Services.GetRequiredService<TimbnDiscord>();
        await timbnDiscord.Init();
        return app;
    }
}
