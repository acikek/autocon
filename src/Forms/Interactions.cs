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

	public static Modal BuildDenialModal(string rep, ApplicationModel app)
		=> new ModalBuilder()
			.WithTitle($"Deny '{rep}'")
			.WithCustomId($"denial_{app.AppId}")
			.AddTextInput(new TextInputBuilder()
				.WithLabel("Denial Reason")
				.WithRequired(true)
				.WithStyle(TextInputStyle.Short)
				.WithCustomId("reason"))
			.Build();
	
	public static async Task Complete(IDiscordInteraction interaction, ApplicationModel app, Form form, BotContext context)
	{
		app.InProgress = false;

		var embed = form.GenerateResponseBuilder(interaction.User, app.GetFormSectionResponses()).Build();

		await interaction.RespondAsync($"Your **{form.Data.Title}** form has been recorded.\nBelow is a copy of your results.", embed: embed, ephemeral: true);

		var channel = context.GetGuild().GetTextChannel(context.Config.AdminChannelId);
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

	public static Action<MessageProperties> GetMessageUpdate(bool accepted)
		=> (x) =>
		{
			string status = accepted ? "ACCEPTED" : "DENIED";
			x.Content = $"This application has been **{status}**";
			x.Components = BuildResultButtons(false);
		};

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
				await button.UpdateAsync(GetMessageUpdate(true));
				await ApplyResult(button, db, app, null, context);
			}
			else
			{
				string rep = context.Forms[app.FormId].GetRepresentativeData(app.Responses);
				await button.RespondWithModalAsync(BuildDenialModal(rep, app));
			}

			await db.SaveChangesAsync();
		}
	}

	public static async Task HandleDenialModal(SocketModal modal, BotContext context)
	{
		var id = modal.Data.CustomId;
		
		if (!id.StartsWith("denial_"))
			return;

		var appId = Guid.Parse(id.Split("denial_", 2)[1]);
		
		using (var db = new AutoConDatabase())
		{
			var app = await db.Applications.FindAsync(appId);

			if (app is null)
				return;

			await modal.UpdateAsync(GetMessageUpdate(false));
			await ApplyResult(modal, db, app, modal.Data.Components.First().Value, context);
			await db.SaveChangesAsync();
		}
	}

	public static async Task ApplyResult(IDiscordInteraction interaction, AutoConDatabase db, ApplicationModel app, string? denialReason, BotContext context)
	{
		bool accepted = denialReason is null;
		
		var user = await context.Client.GetUserAsync(app.UserId);
		var form = context.Forms[app.FormId];
		var rep = form.GetRepresentativeData(app.Responses);

		string status = accepted ? "accepted" : "denied";
		string message = $"Your **{form.Data.Title}** application ({rep}) has been **{status}**!";
		
		if (denialReason is not null)
		{
			message += $"\nReason: {denialReason}";
			message += "\nPlease submit another application with corrections.";
		}

		await user.SendMessageAsync(message);

		if (accepted)
		{
			app.Accepted = true;
			var guildUser = context.GetGuild().GetUser(app.UserId);
			if (!guildUser.Roles.Where(x => x.Id == context.Config.ParticipantRoleId).Any())
				await guildUser.AddRoleAsync(context.Config.ParticipantRoleId);
		}
		else
		{
			db.Applications.Remove(app);
		}
	}

	public static void Register(BotContext context)
	{
		context.Client.ModalSubmitted += (modal) => NextQuery(modal, modal.Data, modal.Data.CustomId, context);
		context.Client.SelectMenuExecuted += (menu) => NextQuery(menu, menu.Data, menu.Data.CustomId, context);

		context.Client.ButtonExecuted += (button) => HandleResultButton(button, context);
		context.Client.ModalSubmitted += (modal) => HandleDenialModal(modal, context);
	}
}
