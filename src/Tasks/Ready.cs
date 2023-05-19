using Discord;
using Discord.Net;
using Discord.WebSocket;
using Models;
using Newtonsoft.Json;
using Structs;

namespace Tasks;

public class Ready {

	public static async Task OnReady(DiscordSocketClient client, Config config) {
		var guild = client.GetGuild(config.GuildId);

		var phase = new SlashCommandOptionBuilder()
			.WithName("phase")
			.WithDescription("The phase to advance to")
			.WithType(ApplicationCommandOptionType.Integer)
			.WithRequired(true);
		Phases.AddAllToOption(phase);

		var progress = new SlashCommandOptionBuilder()
			.WithName(Commands.ADMIN_PROGRESS)
			.WithDescription("Progresses the current con phase")
			.WithType(ApplicationCommandOptionType.SubCommand)
			.AddOptions(phase);

		var admin = new SlashCommandBuilder()
			.WithName(Commands.ADMIN)
			.WithDescription("Admin control panel")
			.WithDefaultPermission(false)
			.AddOption(progress);

		try
		{
			await guild.CreateApplicationCommandAsync(admin.Build());
		}
		catch (HttpException exception)
		{
			var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
			Console.WriteLine(json);
		}
	}
}