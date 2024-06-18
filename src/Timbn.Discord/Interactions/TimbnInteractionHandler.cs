using System.Reflection;

namespace Timbn.Discord.Interactions;

public interface ITimbnInteractionHandler
{
    internal void AddLogger(Func<LogMessage, Task> logger);
    internal Task InitializeAsync(TimbnDiscordOptions options);
}

public abstract class TimbnInteractionHandler : ITimbnInteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;

    public TimbnInteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
    {
        _client = client;
        _handler = handler;
        _services = services;
    }

    void ITimbnInteractionHandler.AddLogger(Func<LogMessage, Task> logger) => _handler.Log += logger;

    async Task ITimbnInteractionHandler.InitializeAsync(TimbnDiscordOptions options)
    {
        if (options.DevDiscordGuidId is not null)
        {
            _client.Ready += async () =>
            {
                await _handler.RegisterCommandsToGuildAsync(options.DevDiscordGuidId.Value);
            };
        }

        var interactionTypes = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.GetCustomAttribute<GroupAttribute>() is not null);

        var assemblies = interactionTypes
            .Select(x => x.Assembly)
            .Distinct();

        foreach (var assembly in assemblies)
        {
            await _handler.AddModulesAsync(assembly, _services);
        }

        _client.InteractionCreated += HandleInteractionCreated;
        _handler.InteractionExecuted += HandleInteractionExecuted;
    }

    private async Task HandleInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _handler.ExecuteCommandAsync(context, _services);
            if (!result.IsSuccess)
                await HandleInteractionCreatedErrorAsync(result);
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction
                    .GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private async Task HandleInteractionExecuted(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        await HandleInteractionExecutedAsync(result);
        if (!result.IsSuccess)
            await HandleInteractionExecutedErrorAsync(result);
    }

    public virtual Task HandleInteractionCreatedErrorAsync(IResult result) => Task.CompletedTask;
    public virtual Task HandleInteractionExecutedAsync(IResult result) => Task.CompletedTask;
    public virtual Task HandleInteractionExecutedErrorAsync(IResult result) => Task.CompletedTask;
}
