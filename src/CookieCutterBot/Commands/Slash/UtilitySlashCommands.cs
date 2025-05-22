using Discord.Interactions;

namespace CookieCutterBot.Commands.Slash;

public class UtilitySlashCommands : InteractionModuleBase
{
    [SlashCommand("ping",
        "Test bot response.",
        true,
        RunMode.Async)]
    private async Task PingAsync()
    {
        await RespondAsync("Pong!");
    }
}