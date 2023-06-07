using Discord.WebSocket;
using Forms;
using Structs;

public record BotContext(DiscordSocketClient Client, Config Config, Properties Properties, FormManager Forms)
{
	public SocketGuild GetGuild()
		=> this.Client.GetGuild(this.Config.GuildId);
}