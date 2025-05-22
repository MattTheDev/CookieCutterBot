using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace CookieCutterBot.Services
{
    public class DiscordBotService(ILogger<DiscordBotService> logger,
DiscordSocketClient discordSocketClient,
IServiceProvider serviceProvider,
    InteractionService interactionService,
    CommandService commandService) 
        : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartConnectionAsync();
            await ValidateBotConnection(cancellationToken);
            await InitializeCommands();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Bot stopping");
            return Task.CompletedTask;
        }

        private async Task ValidateBotConnection(CancellationToken cancellationToken)
        {
            while (discordSocketClient.CurrentUser == null)
            {
                logger.LogInformation("Discord user connection pending ...");
                await Task.Delay(5000, cancellationToken);
            }

            while (discordSocketClient.ConnectionState != ConnectionState.Connected)
            {
                logger.LogInformation("Discord user connection pending ...");
                await Task.Delay(5000, cancellationToken);
            }

            logger.LogInformation($"Discord user connected: {discordSocketClient.CurrentUser.Username}");
        }

        private async Task StartConnectionAsync()
        {
                logger.LogInformation("Starting connection to Discord ...");
            var discordToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

                if (string.IsNullOrWhiteSpace(discordToken))
                {
                    logger.LogCritical("Bot token missing for the `BotSettings.json` file. Please enter the bot token, and restart the service.");

                    throw new Exception("Please enter your bot's token into the `BotSettings.json` file found in the applications root directory.");
                }
                await discordSocketClient.LoginAsync(TokenType.Bot, discordToken);
                await discordSocketClient.StartAsync();

                logger.LogInformation("Connection to Discord Established ...");
        }

        private async Task InitializeCommands()
        {
            await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(),
                services: serviceProvider);
            await commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                services: serviceProvider);

            await interactionService.RegisterCommandsGloballyAsync();

            discordSocketClient.SlashCommandExecuted += async interaction =>
            {
                var ctx = new SocketInteractionContext<SocketSlashCommand>(discordSocketClient, interaction);
                await interactionService.ExecuteCommandAsync(ctx, serviceProvider);
            };

            discordSocketClient.MessageReceived += async socketMessage =>
            {
                if (socketMessage is not SocketUserMessage message)
                {
                    return;
                }

                var argPos = 0;

                if (!(message.HasMentionPrefix(discordSocketClient.CurrentUser, ref argPos)) || message.Author.IsBot)
                {
                    return;
                }

                var context = new SocketCommandContext(discordSocketClient, message);

                await commandService.ExecuteAsync(
                        context: context,
                        argPos: argPos,
                        services: serviceProvider)
                    .ConfigureAwait(false);
            };
        }
    }
}
