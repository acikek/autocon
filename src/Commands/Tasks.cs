using Discord.Net;
using Discord.WebSocket;
using Models;
using Newtonsoft.Json;

namespace Commands;

public class Tasks {

	public static async Task OnReady(DiscordSocketClient client, Config config) {
		var guild = client.GetGuild(config.GuildId);

		try
		{
			await guild.CreateApplicationCommandAsync(Commands.ADMIN_BUILDER.Build());
		}
		catch (HttpException exception)
		{
			var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
			Console.WriteLine(json);
		}
	}

	public static async Task OnCommand(SocketSlashCommand command, Properties properties) {
		switch (command.Data.Name) {
			case Commands.ADMIN: 
				await Handlers.HandleAdmin(command, properties);
				break;
		}
	}

	public static async Task OnButton(SocketMessageComponent component, Properties properties) {
		if (component.Data.CustomId.StartsWith("progress")) {
			await Handlers.HandleProgress(component, properties);
		}
	}
}
