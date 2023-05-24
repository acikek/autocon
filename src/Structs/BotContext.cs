using Discord.WebSocket;
using Structs;

public record BotContext(DiscordSocketClient Client, Config Config, Properties Properties);