using Discord;
using Discord.WebSocket;

namespace Forms;

/// <summary>
///	A deserializable selection menu choice that generates a <see cref="SelectMenuOptionBuilder"/>.
/// </summary>
public record SelectionChoice(string Title, string Description, string Id, string Emoji)
{

	/// <summary>
	/// Generates a selection menu builder based on the provided data.
	/// </summary>
	public SelectMenuOptionBuilder GenerateBuilder()
		=> new SelectMenuOptionBuilder()
			.WithLabel(this.Title)
			.WithDescription(this.Description)
			.WithValue(this.Id)
			.WithEmote(new Emoji(this.Emoji));
}

/// <summary>
/// A form selection menu containing a list of <see cref="SelectionChoice"/>s
/// attached to a prompt and a response title.
/// </summary>
public record FormSelection(string Title, List<SelectionChoice> Choices) : FormQuery
{
	/// <inheritdoc/>
	public override async Task Display(IDiscordInteraction interaction, QueryContext context)
	{
		var builder = new SelectMenuBuilder()
			.WithPlaceholder(this.Title)
			.WithCustomId(context.GetComponentId());
		
		foreach (var choice in this.Choices)
		{
			builder.AddOption(choice.GenerateBuilder());
		}

		var components = new ComponentBuilder()
			.WithSelectMenu(builder)
			.Build();

		await interaction.RespondAsync(components: components);
	}

	/// <inheritdoc/>
	public override List<FormSectionResponse> GetResponseData(IDiscordInteractionData rawData, QueryContext context)
	{
		var result = new List<FormSectionResponse>();

		if (rawData is SocketMessageComponentData data)
		{
			var choice = this.Choices.Find(x => x.Id == data.Values.First());
			result.Add(new FormSectionResponse(this.Title, choice?.Title ?? "Unknown"));
		}

		return result;
	}
}
