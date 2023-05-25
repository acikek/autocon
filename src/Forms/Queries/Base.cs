using Database;
using Discord;

namespace Forms;

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
/// </summary>
public abstract record FormQuery
{

	/// <summary>
	/// The prefix for queryable component IDs.
	/// </summary>
	public const string PREFIX = "form";

	/// <summary>
	/// Responds and displays this form query to an interaction user.
	/// </summary>
	public virtual Task Display(IDiscordInteraction interaction, QueryContext context)
		=> Task.CompletedTask;

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
public record FormSectionResponse(string Title, string Value)
{
	/// <returns>
	/// A form response with no user input.
	/// </returns>
	public static FormSectionResponse None(string title)
		=> new FormSectionResponse(title, "*No response*");

	/// <returns>
	///	A normal form response if <c>value</c> contains user input.
	/// Otherwise, an empty form response using <see cref="FormSectionResponse.None(string)"/>.
	/// </returns>
	public static FormSectionResponse FromPossiblyEmpty(string title, string value)
		=> String.IsNullOrWhiteSpace(value)
			? None(title)
			: new FormSectionResponse(title, value);

	public static explicit operator FormResponseModel(FormSectionResponse response)
		=> new FormResponseModel {
			Title = response.Title,
			Value = response.Value
		};
}
