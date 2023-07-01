using Database;
using Discord;
using Discord.WebSocket;
using Forms;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public static class Applications
{

	public const string NAME = "applications";
	
	public const string SUB_OVERVIEW = "overview";
	public const string SUB_USER = "user";

	public const string OPT_USER = "user";
	public const string OPT_FORM = "form";

	public static SlashCommandBuilder GetCommand(BotContext context)
		=> new SlashCommandBuilder()
			.WithName(NAME)
			.WithDescription("View submitted application info")
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(SUB_OVERVIEW)
				.WithDescription("View an overview of all submitted applications")
				.WithType(ApplicationCommandOptionType.SubCommand))
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(SUB_USER)
				.WithDescription("View a user's submitted applications")
				.WithType(ApplicationCommandOptionType.SubCommand)
					.AddOption(new SlashCommandOptionBuilder()
						.WithName(OPT_USER)
						.WithDescription("The user to query")
						.WithRequired(true)
						.WithType(ApplicationCommandOptionType.User))
					.AddOption(new SlashCommandOptionBuilder()
						.WithName(OPT_FORM)
						.WithDescription("The form to check submissions for")
						.WithRequired(true)
						.WithType(ApplicationCommandOptionType.String)
						.AddFormChoices(context)));

	private static Embed BuildOverviewEmbed(BotContext context)
	{
		var firstForm = context.Forms.All.First().Value;
		var embed = new EmbedBuilder()
			.WithTitle($"{context.Config.ConName} Applications")
			.WithColor(firstForm.Data.Color);

		using (var db = new AutoConDatabase())
		{
			foreach (var (id, form) in context.Forms.All)
			{
				var apps = db.Applications.Where(x => x.FormId == id);
				var accepted = apps.Where(x => x.Accepted);

				var value = $"`{apps.Count()}` total - `{accepted.Count()}` accepted - `{apps.Count() - accepted.Count()}` outgoing";
				embed.AddField(form.Data.Title, value);
			}
		}

		return embed.Build();
	}

	public static async Task Handle(SocketSlashCommand command, BotContext context) 
	{
		var sub = command.Data.Options.First();

		switch (sub.Name)
		{
			case SUB_OVERVIEW:
				await command.RespondAsync(embed: BuildOverviewEmbed(context), ephemeral: true);
				return;
			case SUB_USER:
				await HandleUser(command, sub, context);
				return;
		}
	}

	private static async Task HandleUser(SocketSlashCommand command, SocketSlashCommandDataOption sub, BotContext context)
	{
		var user = (IUser) sub.Options.Where(opt => opt.Name == OPT_USER).First().Value;
		bool isSelf = command.User.Id == user.Id;

		var formId = (string) sub.Options.Where(opt => opt.Name == OPT_FORM).First().Value;
		var form = context.Forms[formId];

		if (!isSelf && !context.IsUserElevated(command) && !form.Data.Visible)
		{
			await command.RespondAsync("You do not have permission to view this form type.");
			return;
		}

		using (var db = new AutoConDatabase())
		{
			var formModel = db.FindForm(formId);
			var apps = formModel?.Applications.Where(x => x.UserId == user.Id);

			if (apps is null)
				return;

			var app = apps.First();

			var embed = form.GenerateResponseBuilder(user, app.GetFormSectionResponses()).Build();

			await command.RespondAsync(embed: embed);
		}
	}
}