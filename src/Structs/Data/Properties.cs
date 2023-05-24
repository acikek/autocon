using Newtonsoft.Json;
using Structs;

namespace Structs;

public class Properties 
{

	public const string PATH = "data/properties.json";

	public Phase Phase { get; set; }

	public async Task UpdateActivity(BotContext context) 
		=> await context.Client.SetActivityAsync(this.Phase.GetActivity(context));

	public async Task ProgressPhase(BotContext context) 
	{
		this.Phase += 1;
		await UpdateActivity(context);
	}

	public static Properties Read() 
	{
		if (!File.Exists(PATH)) {
			var properties = new Properties {
				Phase = Phase.Planning
			};
			properties.Write();
			return properties;
		}
		string json = File.ReadAllText(PATH);
		var value = JsonConvert.DeserializeObject<Properties>(json);
		if (value is null) {
			throw new NullReferenceException("Property storage cannot be null");
		}
		return (Properties) value;
	}

	public void Write() 
	{
		var json = JsonConvert.SerializeObject(this);
		File.WriteAllText(PATH, json);
	}
}
