using Newtonsoft.Json;

namespace Models;

public record Config(string ConName, ulong GuildId) 
{
	
	public const string PATH = "resources/config.json";

	public static Config Read() 
	{
		string json = File.ReadAllText(PATH);
		var value = JsonConvert.DeserializeObject<Config>(json);
		if (value is null) {
			throw new NullReferenceException("Config cannot be null");
		}
		return (Config) value;
	}
}
