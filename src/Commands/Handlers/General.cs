using Discord;
using Discord.WebSocket;

namespace Commands.Handlers;

public static class General
{

	public static async Task HandleFormSelection(SocketMessageComponent component, BotContext context)
	{
		switch (component.Data.CustomId)
		{
			case Admin.SELECT_FORM_CHOICE:
				var value = component.Data.Values.First();
				await context.Forms[value].BeginApplication(component);
				break;
		}
	}
}
