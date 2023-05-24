using Commands.Handlers;
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
			await guild.CreateApplicationCommandAsync(Admin.GetCommand(context).Build());
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
			case Admin.NAME: 
				await Admin.Handle(command, context);
				break;
		}
	}

	public static async Task OnButton(SocketMessageComponent component, Context context) 
	{
		if (component.Data.CustomId.StartsWith(Admin.BUTTON_PROGRESS)) 
		{
			await Admin.HandleProgress(component, context);
		}
	}

	public static async Task OnModal(SocketModal modal, Context context)
	{
		/*var (purpose, id) = ModalPurposes.Parse(modal.Data.CustomId);
		switch (purpose)
		{
			case ModalPurpose.Form:
			case ModalPurpose.Testing:
				var builder = Modals.FromId(id).GenerateResponseEmbed(modal);
				await modal.RespondAsync(embed: builder.Build());
				break;*/
			//case ModalPurpose.Form:
			//	await General.HandlePostForm(modal, id, context);
			//	break;
		//}
	}

	public static async Task OnSelectMenu(SocketMessageComponent component, Context context)
	{
		switch (component.Data.CustomId)
		{
			case Admin.SELECT_FORM_CHOICE:
				await General.HandleFormSelection(component, context);
				break;
		}
	}
}
