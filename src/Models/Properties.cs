using System.Text.Json;
using Structs;

namespace Models;

public class Properties {

	public const string PATH = "data/properties.json";

	public Phase Phase { get; set; }

	public void ProgressPhase() {
		this.Phase += 1;
	}

	public static Properties Read() {
		if (!File.Exists(PATH)) {
			var properties = new Properties {
				Phase = Phase.Planning
			};
			properties.Write();
			return properties;
		}
		string json = File.ReadAllText(PATH);
		var value = JsonSerializer.Deserialize<Properties>(json);
		if (value is null) {
			throw new NullReferenceException("Property storage cannot be null");
		}
		return (Properties) value;
	}

	public void Write() {
		var json = JsonSerializer.Serialize(this, typeof(Properties));
		File.WriteAllText(PATH, json);
	}
}
