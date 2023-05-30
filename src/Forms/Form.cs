using Database;
using Discord;
using Newtonsoft.Json;

namespace Forms;

using FormResponseData = IEnumerable<FormResponseModel>;

/// <summary>
/// A representation of the form data file to be deserialized.
/// Not yet attached to an ID.
/// </summary>
public record FormData(string Title, string Description, uint Color, string Emoji, List<FormQuery> Queries);

/// <summary>
///	An advancable list of queryable objects to display to the user.
/// Able to collect and build response data from all individual query responses.
/// </summary>
public record Form(string Title, string Id, string Description, uint Color, string Emoji, List<FormQuery> Queries) : FormData(Title, Description, Color, Emoji, Queries)
{

	/// <summary>
	/// Initializes the form data.
	/// Should be called once after construction.
	/// </summary>
	public void Init()
	{
		for (uint i = 0; i < this.Queries.Count(); i++)
		{
			try
			{
				var query = this.GetQuery(i);
				if (query.Merge is not null)
				{
					query.MergeWith(this.GetQuery((uint) query.Merge));
				}
			}
			catch (Exception e)
			{
				throw new Exception($"Error initializing query {i}", e);
			}
		}
	}

	/// <summary>
	/// Constructs query context based on a query object index.
	/// <seealso cref="QueryContext"/>
	/// </summary>
	public QueryContext GetContext(uint index)
		=> new QueryContext(this, index);

	/// <returns>
	/// Whether the specified query index exists in the <see cref="Form.Queries"/> list.
	/// </returns>
	public bool HasQuery(uint index) 
		=> index < this.Queries.Count();

	/// <returns>
	/// The query at the specified index, if any.
	/// </returns>
	public FormQuery GetQuery(uint index)
		=> this.Queries.ElementAt((int) index);

	/// <returns>
	///	The next query that can be displayed given the current response data.
	/// This is <c>null</c> if there aren't any queries left or none of them are displayable.
	/// </returns>
	public uint? GetNextQuery(uint currentQuery, FormResponseData responseData)
	{
		for (uint i = currentQuery + 1; i < this.Queries.Count(); i++)
		{
			if (!HasQuery(i))
				return null;

			var query = GetQuery(i);

			if (query.CanDisplay(responseData))
				return i;
		}
		return null;
	}

	public async Task DisplayQuery(IDiscordInteraction interaction, uint index)
		=> await GetQuery(index).Display(interaction, GetContext(index));

	public List<FormSectionResponse> GetQueryResponseData(IDiscordInteractionData data, uint index)
		=> GetQuery(index).GetResponseData(data, GetContext(index));

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

	public async Task BeginApplication(IDiscordInteraction interaction)
	{
		var index = await StartApplicationIfNotPresent(interaction.User);
		await DisplayQuery(interaction, index);
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

		var form = new Form(value.Title, id, value.Description, value.Color, value.Emoji, value.Queries);

		try
		{
			form.Init();
		}
		catch (Exception e)
		{
			throw new Exception($"Error initializing form '{id}'", e);
		}

		return form;
	}
}
