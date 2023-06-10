using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Forms;

/// <summary>
///	A deserializable selection menu choice that generates a <see cref="SelectMenuOptionBuilder"/>.
/// </summary>
[JsonObject(ItemRequired = Required.AllowNull)]
public record SelectionChoice(string Title, string? Description, string Id, string Emoji)
{

	/// <summary>
	/// Generates a selection menu builder based on the provided data.
	/// </summary>
	public SelectMenuOptionBuilder GenerateBuilder()
	{
		var builder = new SelectMenuOptionBuilder()
			.WithLabel(this.Title)
			.WithValue(this.Id)
			.WithEmote(new Emoji(this.Emoji));
		if (this.Description is not null)
		{
			builder.WithDescription(this.Description);
		}
		return builder;
	}
}

/// <summary>
/// A form selection menu containing a list of <see cref="SelectionChoice"/>s
/// attached to a prompt and a response title.
/// </summary>
[JsonObject(ItemRequired = Required.DisallowNull)]
public record FormSelection(string Title, uint? Merge, string? Message, List<SelectionChoice> Choices, List<string>? Conditions) : FormQuery(Merge, Conditions)
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

		await interaction.RespondAsync(this.Message, components: components, ephemeral: true);
	}

	/// <inheritdoc/>
	public override void MergeWith(FormQuery other)
	{
		if (other is FormSelection selection)
		{
			this.Choices.AddRange(selection.Choices);
		}
		else
		{
			throw new InvalidOperationException("Merge type must be 'selection'");
		}
	}

	/// <inheritdoc/>
	public override List<FormSectionResponse> GetResponseData(IDiscordInteractionData rawData, QueryContext context)
	{
		var result = new List<FormSectionResponse>();

		if (rawData is SocketMessageComponentData data)
		{
			var choice = this.Choices.Find(x => x.Id == data.Values.First());
			result.Add(new FormSectionResponse(this.Title, choice.Id, choice.Title));
		}

		return result;
	}
}
