using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forms;

/// <summary>
/// A JSON converter that can deserialize <see cref="FormQuery"/> implementations
/// from the same objects using an extra <c>Type</c> field.
/// </summary>
public class QueryConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
		=> objectType == typeof(FormQuery);

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var obj = JObject.Load(reader);
		var type = obj["Type"]?.Value<string>();

		if (type is null)
		{
			throw new JsonException("Form query 'Type' field must not be null");
		}

		return type switch
		{
			"modal" => obj.ToObject<FormModal>(serializer),
			"selection" => obj.ToObject<FormSelection>(serializer),
			_ => null
		};
	}

	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		=> throw new NotImplementedException();
}