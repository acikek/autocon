using Database;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Forms;

public static class FormInteractions
{

	public const string ACCEPT = $"{FormQuery.PREFIX}_accept";
	public const string DENY = $"{FormQuery.PREFIX}_deny";

	public static MessageComponent BuildResultButtons(bool enabled)
		=> new ComponentBuilder()
			.WithButton(new ButtonBuilder()
				.WithStyle(ButtonStyle.Success)
				.WithLabel("Accept")
				.WithCustomId(ACCEPT)
				.WithDisabled(!enabled))
			.WithButton(new ButtonBuilder()
				.WithStyle(ButtonStyle.Danger)
				.WithLabel("Deny")
				.WithCustomId(DENY)
				.WithDisabled(!enabled))
			.Build();
	
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

		var message = await channel.SendMessageAsync(embed: embed, components: BuildResultButtons(true));
		app.MessageId = message.Id;
	}

	public static async Task NextQuery(IDiscordInteraction interaction, IDiscordInteractionData data, string componentId, BotContext context)
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

	public static async Task HandleResultButton(SocketMessageComponent button, BotContext context)
	{
		var id = button.Data.CustomId;

		if (!FormQuery.IsFormQuery(id))
			return;
		
		using (var db = new AutoConDatabase())
		{
			var app = db.Applications
				.Where(x => x.MessageId == button.Message.Id)
				.Include(x => x.Responses)
				.FirstOrDefault();

			if (app is null)
				return;
			
			if (id == ACCEPT)
			{
				await button.UpdateAsync(x => 
				{
					x.Content = "This application has been **ACCEPTED**";
					x.Components = BuildResultButtons(false);
				});

				app.Accepted = true;
				
				var user = await context.Client.GetUserAsync(app.UserId);
				var form = context.Forms[app.FormId];
				var rep = form.GetRepresentativeData(app.Responses);
				await user.SendMessageAsync($"Your **{form.Title}** application ({rep}) has been accepted!");
			}

			await db.SaveChangesAsync();
		}
	}

	public static void Register(BotContext context)
	{
		context.Client.ModalSubmitted += (modal) => NextQuery(modal, modal.Data, modal.Data.CustomId, context);
		context.Client.SelectMenuExecuted += (menu) => NextQuery(menu, menu.Data, menu.Data.CustomId, context);
		context.Client.ButtonExecuted += (button) => HandleResultButton(button, context);
	}
}
