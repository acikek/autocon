using Discord;
using Discord.WebSocket;
using Forms;
using Structs;

public record BotContext(DiscordSocketClient Client, Config Config, Properties Properties, FormManager Forms)
{
	public SocketGuild GetGuild()
		=> this.Client.GetGuild(this.Config.GuildId);

	public bool IsUserElevated(SocketInteraction interaction)
		=> interaction.User is SocketGuildUser guildUser 
			&& guildUser.Roles.Where(role => role.Id == this.Config.OrganizerRoleId).Any();
}