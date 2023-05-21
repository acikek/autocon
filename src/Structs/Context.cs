using Discord.WebSocket;
using Models;

public record Context(DiscordSocketClient Client, Config Config, Properties Properties);