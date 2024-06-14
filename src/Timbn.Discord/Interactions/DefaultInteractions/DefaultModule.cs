using Discord.Interactions;

namespace Timbn.Discord.Interactions.DefaultInteractions;

[Group("default", "Default Commands")]
public class DefaultModule : InteractionModuleBase
{
    [SlashCommand("ping", "Ping Pong")]
    public async Task Ping([Summary(description: "Message to Pong")] string message = "")
    {
        await RespondAsync($"Pong {message}", ephemeral: true);
    }
}
