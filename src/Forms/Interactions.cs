using Database;
using Discord;

namespace Forms;

public static class FormInteractions
{
	
	public static async Task Complete(IDiscordInteraction interaction, ApplicationModel app, Form form, BotContext context)
	{
		app.Responses.Sort((x, y) => x.Index.CompareTo(y.Index));
		app.InProgress = false;

		var allResponses = app.Responses.Select(x => x.Revert()).ToList();
		var embed = form.GenerateResponseBuilder(interaction, allResponses).Build();

		await interaction.RespondAsync($"Your **{form.Title}** form has been recorded.\nBelow is a copy of your results.", embed: embed, ephemeral: true);
		
		var channel = context.Client
			.GetGuild(context.Config.GuildId)
			.GetTextChannel(context.Config.FormSubmissionChannelId);
		await channel.SendMessageAsync(embed: embed);
	}

	public static async Task Next(IDiscordInteraction interaction, IDiscordInteractionData data, string componentId, BotContext context)
	{
		if (!FormQuery.IsFormQuery(componentId))
			return;

		var (form, index) = context.Forms.Parse(componentId);

		using (var db = new AutoConDatabase()) 
		{
			var formData = db.FindForm(form.Id);
			var app = formData?.FindResumable(interaction.User.Id);

			if (app is null)
				return;

			if (app.CurrentQuery != index)
			{
				await interaction.RespondAsync("Invalid query!", ephemeral: true);
				return;
			}

			var responses = form.GetQueryResponseData(data, index);
			var responseData = app.GetResponseData(responses);

			app.Responses.AddRange(responseData);

			var next = form.GetNextQuery(app.CurrentQuery, app.Responses);
			
			if (next is not null)
			{
				uint value = (uint) next;
				app.CurrentQuery = value;
				await form.DisplayQuery(interaction, value);
			}
			else
			{
				await Complete(interaction, app, form, context);
			}

			await db.SaveChangesAsync();
		}
	}

	public static void Register(BotContext context)
	{
		context.Client.ModalSubmitted += (modal) => Next(modal, modal.Data, modal.Data.CustomId, context);
		context.Client.SelectMenuExecuted += (menu) => Next(menu, menu.Data, menu.Data.CustomId, context);
	}
}