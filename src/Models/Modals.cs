using System.Runtime.Serialization;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Models;

public enum ModalPurpose
{
	Testing,
	Form
}

public static class ModalPurposes
{
	public static string GetId(this ModalPurpose purpose)
		=> Enum.GetName<ModalPurpose>(purpose)?.ToLower() ?? "unknown";

	public static string Prefix(this ModalPurpose purpose, string id)
		=> $"{purpose.GetId()}_{id}";

	public static (ModalPurpose, string) Parse(string fullId)
	{
		var elements = fullId.Split("_", 2);
		return (Enum.Parse<ModalPurpose>(elements[0], true), elements[1]);
	}
}

[JsonConverter(typeof(StringEnumConverter))]
public enum SectionType
{
	[EnumMember(Value = "short")]
	Short,
	[EnumMember(Value = "long")]
	Long
}

public record Section(string Title, SectionType Type, string Id, string Placeholder, bool Required)
{
	public const int MAX_SIZE = 200;

	public TextInputStyle GetInputStyle()
		=> this.Type == SectionType.Short 
			? TextInputStyle.Short 
			: TextInputStyle.Paragraph;

	public TextInputBuilder GenerateBuilder()
		=> new TextInputBuilder()
			.WithLabel(this.Title)
			.WithStyle(GetInputStyle())
			.WithCustomId(this.Id)
			.WithPlaceholder(this.Placeholder)
			.WithRequired(this.Required)
			.WithMaxLength(MAX_SIZE);
}

public record ModalData(string Title, uint Color, List<Section> Sections);

public record GeneratedModal(string Title, string Id, uint Color, List<Section> Sections)
{
	public Modal Generate(ModalPurpose purpose)
	{
		var builder = new ModalBuilder()
			.WithTitle(this.Title)
			.WithCustomId(purpose.Prefix(this.Id));
		foreach (var section in this.Sections)
		{
			builder.AddTextInput(section.GenerateBuilder());
		}
		return builder.Build();
	}

	public async Task Display(IDiscordInteraction interaction, ModalPurpose purpose)
		=> await interaction.RespondWithModalAsync(Generate(purpose));

	public EmbedBuilder GenerateResponseEmbed(SocketModal modal)
	{
		var builder = new EmbedBuilder()
			.WithTitle($"New {this.Title} Response")
			.WithColor(this.Color)
			.WithAuthor(new EmbedAuthorBuilder()
				.WithName($"@{modal.User.Username}")
				.WithIconUrl(modal.User.GetAvatarUrl()))
			.WithCurrentTimestamp();

		for (int i = 0; i < this.Sections.Count(); i++)
		{
			var section = this.Sections[i];
			string value = modal.Data.Components.ElementAt(i).Value;
			string display = String.IsNullOrWhiteSpace(value)
				? "*No response*"
				: value;
			builder.AddField(section.Title, display, inline: false);
		}

		return builder;
	}

	public static GeneratedModal Read(string path, string id) 
	{
		string json = File.ReadAllText(path);
		var value = JsonConvert.DeserializeObject<ModalData>(json);
		if (value is null) {
			throw new NullReferenceException($"Modal '{id}' cannot be null");
		}
		return new GeneratedModal(value.Title, id, value.Color, value.Sections);
	}
}

public class Modals 
{

	public const string LOCATION = "modals";

	public static readonly List<GeneratedModal> ALL = new List<GeneratedModal>();

	public static readonly GeneratedModal BOOTH = Create("booth");
	public static readonly GeneratedModal EVENT = Create("event");

	public static GeneratedModal Create(string id)
	{
		var value = GeneratedModal.Read($"{LOCATION}/{id}.json", id);
		ALL.Add(value);
		return value;
	}

	public static GeneratedModal FromId(string id)
		=> ALL.First(modal => modal.Id == id);

	public static void Init()
	{}
}

public static class SlashCommandOptionBuilderExtensions 
{

	public static SlashCommandOptionBuilder AddModalChoices(this SlashCommandOptionBuilder builder, Context context) 
	{
		foreach (var modal in Modals.ALL)
		{
			builder.AddChoice(modal.Title, modal.Id);
		}
		return builder;
	}
}