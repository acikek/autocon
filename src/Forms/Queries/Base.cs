using Database;
using Discord;

namespace Forms;

using FormResponseData = IEnumerable<FormResponseModel>;

/// <summary>
/// A context object to pass into various <see cref="FormQuery"/> methods.
/// <c>Index</c> is the 0-based index of the query in the form's list.
/// </summary>
public record QueryContext(Form Form, uint Index)
{

	/// <returns>
	/// A component ID in the format of <c>form_{formId}_{index}</c>.
	/// </returns>
	public string GetComponentId()
		=> $"{FormQuery.PREFIX}_{this.Form.Id}_{this.Index}";
}

/// <summary>
/// A form query object that a user can interact with and submit data to.
/// Each query object has an index used for displaying queryable components to users in-order.
/// This index is appended to the component's ID alongside the form ID itself.
///
/// Each query also has a set of condition IDs. Before this query is displayed,
/// these conditions should be met. This can be checked with <see cref="FormQuery.CanDisplay(List{FormResponseModel})"/>.
/// </summary>
public abstract record FormQuery(uint? Merge, List<string>? Conditions)
{

	/// <summary>
	/// The prefix for queryable component IDs.
	/// </summary>
	public const string PREFIX = "form";

	/// <returns>
	///	A tuple with the first element being the condition ID and the second element being whether
	/// the condition is for the allow or denylist.
	/// </returns>
	private static (string, bool) ParseCondition(string condition)
	{
		if (condition.StartsWith("!"))
		{
			var id = condition.Split("!", 2)[1];
			return (id, false);
		}

		return (condition, true);
	}

	/// <returns>
	/// Whether this query can be displayed; in other words, whether the prior responses
	/// contain all the requires IDs specified in <see cref="FormQuery.Conditions"/>.
	/// </returns>
	public bool CanDisplay(FormResponseData responseData)
	{
		if (this.Conditions is null)
			return true;

		foreach (var cond in this.Conditions)
		{
			var (id, accept) = ParseCondition(cond);
			var data = responseData.Where(x => x.OptionId == id);

			if (data.Any() != accept) 
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Responds and displays this form query to an interaction user.
	/// </summary>
	public virtual Task Display(IDiscordInteraction interaction, QueryContext context)
		=> Task.CompletedTask;

	/// <summary>
	/// Merges another form query into this instance.
	/// This is only called if <see cref="FormQuery.Merge"/> is not <c>null</c>. 
	/// </summary>
	public abstract void MergeWith(FormQuery other);

	/// <summary>
	///	Collects and assembles form responses from raw interaction data.
	/// </summary>
	public abstract List<FormSectionResponse> GetResponseData(IDiscordInteractionData rawData, QueryContext context); 

	/// <returns>
	/// Whether the provided component ID matches the form query format.
	/// </returns>
	public static bool IsFormQuery(string componentId)
		=> componentId.StartsWith(PREFIX);

	/// <summary>
	///	Parses the component ID into the form ID and the query index.
	/// Does not perform validation on the component ID. Use <see cref="FormQuery.IsFormQuery(string)"/> for validation.
	/// </summary>
	public static (string, uint) Parse(string componentId)
	{
		var segments = componentId.Split("_", 3);
		return (segments[1], uint.Parse(segments[2]));
	}
}

/// <summary>
/// A response to a form section collected from user input data.
/// </summary>
public record FormSectionResponse(string Title, string Id, string Value)
{
	/// <returns>
	/// A form response with no user input.
	/// </returns>
	public static FormSectionResponse None(string title, string id)
		=> new FormSectionResponse(title, id, "*No response*");

	/// <returns>
	///	A normal form response if <c>value</c> contains user input.
	/// Otherwise, an empty form response using <see cref="FormSectionResponse.None(string, string)"/>.
	/// </returns>
	public static FormSectionResponse FromPossiblyEmpty(string title, string id, string value)
		=> String.IsNullOrWhiteSpace(value)
			? None(title, id)
			: new FormSectionResponse(title, id, value);
}
