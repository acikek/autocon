using Discord.Net;
using Discord.WebSocket;
using Models;
using Newtonsoft.Json;

namespace Commands;

public class Tasks 
{

	public static async Task OnReady(Context context) 
	{
		var guild = context.Client.GetGuild(context.Config.GuildId);

		try
		{
			await guild.CreateApplicationCommandAsync(Commands.GetAdminBuilder(context).Build());
		}
		catch (HttpException exception)
		{
			var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
			Console.WriteLine(json);
		}
	}

	public static async Task OnCommand(SocketSlashCommand command, Context context) 
	{
		switch (command.Data.Name) 
		{
			case Commands.ADMIN: 
				await Handlers.HandleAdmin(command, context);
				break;
		}
	}

	public static async Task OnButton(SocketMessageComponent component, Context context) 
	{
		if (component.Data.CustomId.StartsWith("progress")) 
		{
			await Handlers.HandleProgress(component, context);
		}
	}

	public static async Task OnModal(SocketModal modal, Context context)
	{
		var (purpose, id) = ModalPurposes.Parse(modal.Data.CustomId);
		switch (purpose)
		{
			case ModalPurpose.Testing:
				var builder = context.Modals[id].RespondEmbed(modal);
				await modal.RespondAsync(embed: builder.Build());
				break;
		}
	}
}
