using Discord;
using Discord.WebSocket;
using Forms;
using Structs;

namespace Commands.Handlers;

public static class Admin
{

	public const string NAME = "admin";
	public const string SUB_PROGRESS = "progress";
	public const string SUB_FORM = "form";
	public const string SUB_SUBMISSIONS = "submissions";
	public const string SELECT_FORM_CHOICE = "form_choice";
	public const string BUTTON_PROGRESS = "progress";
	public const string BUTTON_PROGRESS_ACCEPT = "progress_yes";
	public const string BUTTON_PROGRESS_DENY = "progress_no";

	public static SlashCommandBuilder GetCommand(BotContext context)
		=> new SlashCommandBuilder()
			.WithName(NAME)
			.WithDescription("Admin control panel")
			.WithDefaultPermission(false)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(Admin.SUB_PROGRESS)
				.WithDescription("Progresses the current con phase")
				.WithType(ApplicationCommandOptionType.SubCommand))
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(Admin.SUB_FORM)
				.WithDescription("Displays a form and embeds its results")
				.WithType(ApplicationCommandOptionType.SubCommand)
				.AddOption(new SlashCommandOptionBuilder()
					.WithName("id")
					.WithDescription("The ID of the form")
					.WithType(ApplicationCommandOptionType.String)
					.WithRequired(true)
					.AddFormChoices(context)))
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(Admin.SUB_SUBMISSIONS)
				.WithDescription("Bring up the submissions menu")
				.WithType(ApplicationCommandOptionType.SubCommand));

	private static MessageComponent GetProgressionButtons(bool enabled)
		=> new ComponentBuilder()
			.WithButton(new ButtonBuilder()
				.WithStyle(ButtonStyle.Primary)
				.WithLabel("Progress")
				.WithCustomId(BUTTON_PROGRESS_ACCEPT)
				.WithDisabled(!enabled))
			.WithButton(new ButtonBuilder()
				.WithStyle(ButtonStyle.Danger)
				.WithLabel("Abort")
				.WithCustomId(BUTTON_PROGRESS_DENY)
				.WithDisabled(!enabled))
			.Build();

	private static MessageComponent GetSubmissionsSelectMenu(BotContext context)
		=> new ComponentBuilder()
			.WithSelectMenu(new SelectMenuBuilder()
				.WithCustomId(SELECT_FORM_CHOICE)
				.WithPlaceholder($"Submit a {context.Config.ConName} Application")
				.AddOption(new SelectMenuOptionBuilder()
					.WithLabel("Booth")
					.WithValue(General.FORM_CHOICE_BOOTH)
					.WithDescription("Submit a booth to show off a project")
					.WithEmote(new Emoji("\uD83C\uDFE8")))
				.AddOption(new SelectMenuOptionBuilder()
					.WithLabel("Event")
					.WithValue(General.FORM_CHOICE_EVENT)
					.WithDescription("Submit an event for groups to participate in")
					.WithEmote(new Emoji("\uD83C\uDF06"))))
			.Build();

	private static string GetProgressionMessage(Phase nextPhase) 
	{
		var message = $"The convention will progress to the **{nextPhase.GetName()}** phase.";
		var warning = nextPhase.GetAdditionalWarning();
		if (warning is not null) 
		{
			message += $"\nAdditionally, {warning}";
		}
		return message + "\n\nAre you sure about this? **You cannot undo this action!**";
	}

	public static async Task Handle(SocketSlashCommand command, BotContext context) 
	{
		var sub = command.Data.Options.First();

		switch (sub.Name) 
		{
			case SUB_PROGRESS:
				if (context.Properties.Phase.IsFinal()) 
				{
					await command.RespondAsync("The convention is already in the final phase!", ephemeral: true);
					return;
				}
				var message = GetProgressionMessage(context.Properties.Phase.GetNext());
				await command.RespondAsync(message, components: GetProgressionButtons(true), ephemeral: true);
				break;
			case SUB_FORM:
				var formId = (string) sub.Options.First().Value;
				var form = FormManager.ALL[formId];
				var currentQuery = await form.StartApplicationIfNotPresent(command.User);
				await form.DisplayQuery(command, currentQuery);
				break;
			case SUB_SUBMISSIONS:
				await command.RespondAsync(components: GetSubmissionsSelectMenu(context));
				break;
		}
	}

	public static async Task HandleProgress(SocketMessageComponent component, BotContext context) 
	{
		bool confirm = component.Data.CustomId == BUTTON_PROGRESS_ACCEPT;

		if (confirm)
		{
			await context.Properties.ProgressPhase(context);
			context.Properties.Write();
		}

		await component.UpdateAsync(msg =>
		{
			msg.Content = confirm
				? $"The con progressed to the **{context.Properties.Phase.GetName()}** phase successfully."
				: "Progression aborted."; 
			msg.Components = GetProgressionButtons(false);
		});
	}
}
