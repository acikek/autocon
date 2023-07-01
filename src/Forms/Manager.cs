using Discord;

namespace Forms;

public class FormManager
{
	public const string LOCATION = "resources/forms";

	public Dictionary<string, Form> All { get; } = new();

	public FormManager() 
	{
		foreach (var file in Directory.GetFiles(LOCATION))
		{
			var id = Path.GetFileNameWithoutExtension(file);
			var form = Form.Read(file, id);
			All.Add(id, form);
		}
	}

	public QueryContext Parse(string componentId)
	{
		var (formId, index) = FormQuery.Parse(componentId);
		return new QueryContext(this.All[formId], index);
	}

	public Form this[string name]
		=> this.All[name];
}

public static class SlashCommandOptionBuilderExtensions 
{

	public static SlashCommandOptionBuilder AddFormChoices(this SlashCommandOptionBuilder builder, BotContext context) 
	{
		foreach (var form in context.Forms.All.Values)
		{
			builder.AddChoice(form.Data.Title, form.Id);
		}
		return builder;
	}
}
