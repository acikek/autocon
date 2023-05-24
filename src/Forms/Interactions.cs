using Database;
using Discord;

namespace Forms;

public static class FormInteractions
{

	public static async Task Next(IDiscordInteraction interaction, string componentId, BotContext context)
	{
		if (!FormQuery.IsFormQuery(componentId))
			return;

		var (form, index) = FormManager.Parse(componentId);
		var next = form.HasQuery(index + 1);

		using (var db = new AutoConDatabase()) {
			var formData = await db.Applications.FindAsync(interaction.User.Id);

			if (formData is not null)
			{
				var responses = form.GetQueryResponseData(interaction, index)
					.Select(formData.CreateResponse)
					.AsEnumerable();

				formData.CurrentQuery++;
				await db.Responses.AddRangeAsync(responses);
				await db.SaveChangesAsync();

				if (!next)
				{
					var allResponses = db.Responses.Select(x => x.Revert()).ToList();
					var embed = form.GenerateResponseBuilder(interaction, allResponses);
					await interaction.RespondAsync(embed: embed.Build());
				}
			}
		}

		if (next)
		{
			await form.DisplayQuery(interaction, index + 1);
		}
	}

	public static void Register(BotContext context)
	{
		context.Client.ModalSubmitted += (modal) => Next(modal, modal.Data.CustomId, context);
		context.Client.SelectMenuExecuted += (menu) => Next(menu, menu.Data.CustomId, context);
	}
}