using System.Runtime.Serialization;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Models;

public enum ModalPurpose
{
	Testing
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
			.WithMaxLength(200);
}

public record ModalData(string Title, List<Section> Sections);

public record GeneratedModal(string Title, string Id, List<Section> Sections)
{
	public const int EMBED_COLOR = 0xFF0099;

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

	public EmbedBuilder RespondEmbed(SocketModal modal)
	{
		var builder = new EmbedBuilder()
			.WithTitle($"New {this.Title} Response")
			.WithColor(EMBED_COLOR)
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
			builder.AddField(section.Title, display, true);
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
		return new GeneratedModal(value.Title, id, value.Sections);
	}
}

public class Modals 
{

	public const string LOCATION = "resources/modals";

	public Dictionary<string, GeneratedModal> Map { get; } = new Dictionary<string, GeneratedModal>();

	public Modals()
	{
		foreach (var file in Directory.GetFiles(LOCATION))
		{
			string id = Path.GetFileNameWithoutExtension(file);
			Map.Add(id, GeneratedModal.Read(file, id));
		}
	}
	
	public GeneratedModal this[string id]
	{
		get { return Map[id]; }
	}
}

public static class SlashCommandOptionBuilderExtensions 
{

	public static SlashCommandOptionBuilder AddModalChoices(this SlashCommandOptionBuilder builder, Context context) 
	{
		foreach (var (id, modal) in context.Modals.Map)
		{
			builder.AddChoice(modal.Title, id);
		}
		return builder;
	}
}