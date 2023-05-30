using System.Runtime.Serialization;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Forms;

/// <summary>
/// A deserializable modal section type that converts to a <see cref="TextInputStyle"/>.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ModalSectionType
{
	/// <summary>
	/// <seealso cref="TextInputStyle.Short"/>
	/// </summary>
	[EnumMember(Value = "short")]
	Short,
	/// <summary>
	/// <seealso cref="TextInputStyle.Paragraph"/>
	/// </summary>
	[EnumMember(Value = "long")]
	Long
}

public static class ModalSectionTypes
{
	/// <summary>
	/// Converts this section type to a <see cref="Discord"/>-compatible type.
	/// </summary>
	public static TextInputStyle GetInputStyle(this ModalSectionType type)
		=> type == ModalSectionType.Short 
			? TextInputStyle.Short 
			: TextInputStyle.Paragraph;
}

/// <summary>
/// A deserializable modal section that generates a <see cref="TextInputBuilder"/>.
/// </summary>
[JsonObject(ItemRequired = Newtonsoft.Json.Required.Always)]
public record ModalSection(string Title, ModalSectionType Type, string Id, string Placeholder, bool Required)
{
	/// <summary>
	/// The maximum number of characters allowed in a <see cref="ModalSectionType.Long"/> text section.
	/// </summary>
	public const int MAX_SIZE = 200;

	/// <summary>
	/// Generates a text input builder based on the provided data.
	/// </summary>
	public TextInputBuilder GenerateBuilder()
		=> new TextInputBuilder()
			.WithLabel(this.Title)
			.WithStyle(this.Type.GetInputStyle())
			.WithCustomId(this.Id)
			.WithPlaceholder(this.Placeholder)
			.WithRequired(this.Required)
			.WithMaxLength(MAX_SIZE);
}

/// <summary>
/// A form modal, or simply a list of <see cref="ModalSection"/>s attached to a title.
/// </summary>
[JsonObject(ItemRequired = Required.DisallowNull)]
public record FormModal(string? Title, uint? Merge, List<ModalSection> Sections, List<string>? Conditions) : FormQuery(Merge, Conditions)
{
	/// <inheritdoc/>
	public override async Task Display(IDiscordInteraction interaction, QueryContext context)
	{
		var builder = new ModalBuilder()
			.WithTitle(this.Title ?? context.Form.Title)
			.WithCustomId(context.GetComponentId());

		foreach (var section in this.Sections)
		{
			builder.AddTextInput(section.GenerateBuilder());
		}
		
		await interaction.RespondWithModalAsync(builder.Build());
	}

	/// <inheritdoc/>
	public override void MergeWith(FormQuery other)
	{
		if (other is FormModal modal)
		{
			this.Sections.AddRange(modal.Sections);
		}
		else
		{
			throw new InvalidOperationException("Merge type must be a 'modal'");
		}
	}

	/// <inheritdoc/>
	public override List<FormSectionResponse> GetResponseData(IDiscordInteractionData rawData, QueryContext context)
	{
		var result = new List<FormSectionResponse>();

		if (rawData is SocketModalData data)
		{
			for (int i = 0; i < this.Sections.Count(); i++)
			{
				var section = this.Sections[i];
				string value = data.Components.ElementAt(i).Value;
				result.Add(FormSectionResponse.FromPossiblyEmpty(section.Title, section.Id, value));
			}
		}

		return result;
	}
}
