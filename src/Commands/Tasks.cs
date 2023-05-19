using Discord.Net;
using Discord.WebSocket;
using Models;
using Newtonsoft.Json;

namespace Commands;

public class Tasks {

	public static async Task OnReady(Context context) {
		var guild = context.Client.GetGuild(context.Config.GuildId);

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

	public static async Task OnCommand(SocketSlashCommand command, Context context) {
		switch (command.Data.Name) {
			case Commands.ADMIN: 
				await Handlers.HandleAdmin(command, context);
				break;
		}
	}

	public static async Task OnButton(SocketMessageComponent component, Context context) {
		if (component.Data.CustomId.StartsWith("progress")) {
			await Handlers.HandleProgress(component, context);
		}
	}
}
