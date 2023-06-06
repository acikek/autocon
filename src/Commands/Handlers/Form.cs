using Discord;
using Discord.WebSocket;
using Forms;

namespace Commands;

public static class Form
{
	public const string NAME = "form";

	public static SlashCommandBuilder GetCommand(BotContext context)
		=> new SlashCommandBuilder()
			.WithName(NAME)
			.WithDescription("Begins or resumes an application for a form")
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("name")
				.WithDescription("The name of the form")
				.WithType(ApplicationCommandOptionType.String)
				.WithRequired(true)
				.AddFormChoices(context));

	public static async Task Handle(SocketSlashCommand command, BotContext context)
	{
		var formId = (string) command.Data.Options.First().Value;
		await context.Forms[formId].BeginApplication(command);
	}
}