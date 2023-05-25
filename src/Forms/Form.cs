using Database;
using Discord;
using Microsoft.EntityFrameworkCore;
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

	public bool HasQuery(uint index) => index < Queries.Count();

	public async Task DisplayQuery(IDiscordInteraction interaction, uint index)
		=> await Queries.ElementAt((int) index).Display(interaction, GetContext(index));

	public List<FormSectionResponse> GetQueryResponseData(IDiscordInteractionData data, uint index)
		=> Queries.ElementAt((int) index).GetResponseData(data, GetContext(index));

	public async Task<uint> StartApplicationIfNotPresent(IUser user) {
		using (var db = new AutoConDatabase())
		{
			await db.AddFormIfNotPresent(this.Id);
			await db.AddUserIfNotPresent(user.Id);

			var formData = db.FindForm(this.Id);
			var existing = formData?.FindResumable(user.Id);

			if (existing is not null)
				return existing.CurrentQuery;

			formData?.Applications.Add(ApplicationModel.New(this, user));
			await db.SaveChangesAsync();

			return 0;
		}
	}

	public EmbedBuilder GenerateResponseBuilder(IDiscordInteraction interaction, ICollection<FormSectionResponse> responses)
	{
		var builder = new EmbedBuilder()
			.WithTitle($"New {this.Title} Response")
			.WithColor(this.Color)
			.WithAuthor(new EmbedAuthorBuilder()
				.WithName($"@{interaction.User.Username}")
				.WithIconUrl(interaction.User.GetAvatarUrl()))
			.WithCurrentTimestamp();

		for (int i = 0; i < responses.Count(); i++)
		{
			var response = responses.ElementAt(i);
			builder.AddField(response.Title, response.Value, inline: false);
		}

		return builder;
	}

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