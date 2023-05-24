using Discord;
using Newtonsoft.Json;

namespace Forms;

/// <summary>
/// A representation of the form data file to be deserialized.
/// Not yet attached to an ID.
/// </summary>
public record FormData(string Title, uint Color, List<FormQuery> Queries);

/// <summary>
///	
/// </summary>
public record Form(string Title, string Id, uint Color, List<FormQuery> Queries) : FormData(Title, Color, Queries)
{

	public QueryContext GetContext(uint index)
		=> new QueryContext(this, index);

	public async Task DisplayQuery(IDiscordInteraction interaction, uint index)
		=> await Queries.ElementAt((int) index).Display(interaction, GetContext(index));

	public static Form Read(string path, string id) 
	{
		JsonConverter[] converters = { new QueryConverter() };
		var settings = new JsonSerializerSettings() { Converters = converters };

		string json = File.ReadAllText(path);
		var value = JsonConvert.DeserializeObject<FormData>(json, settings);

		if (value is null) {
			throw new NullReferenceException($"Modal '{id}' cannot be null");
		}

		return new Form(value.Title, id, value.Color,value.Queries);
	}
}