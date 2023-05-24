using Discord;

namespace Forms;

public class FormManager
{
	public const string LOCATION = "resources/forms";

	public static readonly Dictionary<string, Form> ALL = new Dictionary<string, Form>();

	public static readonly Form BOOTH = Create("booth");
	public static readonly Form EVENT = Create("event");

	public static Form Create(string id)
	{
		var value = Form.Read($"{LOCATION}/{id}.json", id);
		ALL.Add(id, value);
		return value;
	}

	public static QueryContext Parse(string componentId)
	{
		var (formId, index) = FormQuery.Parse(componentId);
		return new QueryContext(ALL[formId], index);
	}

	public static void Init()
	{}
}

public static class SlashCommandOptionBuilderExtensions 
{

	public static SlashCommandOptionBuilder AddFormChoices(this SlashCommandOptionBuilder builder, BotContext context) 
	{
		foreach (var form in FormManager.ALL.Values)
		{
			builder.AddChoice(form.Title, form.Id);
		}
		return builder;
	}
}
