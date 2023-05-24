using Discord;

namespace Forms;

public class FormManager
{
	public const string LOCATION = "resources/forms";

	public static readonly List<Form> ALL = new List<Form>();

	public static readonly Form BOOTH = Create("booth");
	public static readonly Form EVENT = Create("event");

	public static Form Create(string id)
	{
		var value = Form.Read($"{LOCATION}/{id}.json", id);
		ALL.Add(value);
		Console.WriteLine(id + " " + value);
		return value;
	}

	public static Form FromId(string id)
		=> ALL.First(modal => modal.Id == id);

	public static void Init()
	{}
}

public static class SlashCommandOptionBuilderExtensions 
{

	public static SlashCommandOptionBuilder AddFormChoices(this SlashCommandOptionBuilder builder, Context context) 
	{
		foreach (var form in FormManager.ALL)
		{
			builder.AddChoice(form.Title, form.Id);
		}
		return builder;
	}
}
