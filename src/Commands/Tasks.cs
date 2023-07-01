using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Commands;

public class Tasks 
{

	public static async Task OnReady(BotContext context) 
	{
		var guild = context.GetGuild();

		try
		{
			await guild.CreateApplicationCommandAsync(Admin.GetCommand(context).Build());
			await guild.CreateApplicationCommandAsync(Form.GetCommand(context).Build());
			await guild.CreateApplicationCommandAsync(Applications.GetCommand(context).Build());
			await context.Properties.UpdateActivity(context);
		}
		catch (HttpException exception)
		{
			var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
			Console.WriteLine(json);
		}
	}

	public static async Task OnCommand(SocketSlashCommand command, BotContext context) 
	{
		switch (command.Data.Name) 
		{
			case Admin.NAME: 
				await Admin.Handle(command, context);
				break;
			case Form.NAME:
				await Form.Handle(command, context);
				break;
			case Applications.NAME:
				await Applications.Handle(command, context);
				break;
		}
	}

	public static async Task OnButton(SocketMessageComponent component, BotContext context) 
	{
		if (component.Data.CustomId.StartsWith(Admin.BUTTON_PROGRESS)) 
		{
			await Admin.HandleProgress(component, context);
		}
	}

	public static async Task OnSelectMenu(SocketMessageComponent component, BotContext context)
	{
		switch (component.Data.CustomId)
		{
			case Admin.SELECT_FORM_CHOICE:
				await Admin.HandleFormSelection(component, context);
				break;
		}
	}
}
