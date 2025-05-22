using CookieCutterBot.Services;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Discord.Interactions;

var builder = Host.CreateApplicationBuilder(args);
var socketConfig = new DiscordSocketConfig
{
    LogLevel = LogSeverity.Verbose,
    GatewayIntents =
        GatewayIntents.GuildVoiceStates |
        GatewayIntents.GuildScheduledEvents |
        GatewayIntents.DirectMessages |
        GatewayIntents.GuildIntegrations |
        GatewayIntents.GuildMessageReactions |
        GatewayIntents.Guilds |
        GatewayIntents.GuildMessages,
    UseInteractionSnowflakeDate = false,
};

var client = new DiscordSocketClient(socketConfig);
builder.Services.AddSingleton(client);
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton(new InteractionService(client));

builder.Services.AddHostedService<DiscordBotService>();

var host = builder.Build();
host.Run();
