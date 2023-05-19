using System.Diagnostics;
using Discord;
using Models;

namespace Structs;

public enum Phase {
	Planning,
	Signups,
	Test,
	Build,
	Verify,
	Open
}

public static class PhaseExtensions {

	public static string GetName(this Phase phase)
		=> Enum.GetName(typeof(Phase), phase) ?? "Unknown";

	public static string GetId(this Phase phase)
		=> phase.GetName().ToLower();

	private static IActivity Watching(string status) {
		return new Game(status, ActivityType.Watching);
	}

	private static IActivity Playing(string status) {
		return new Game(status, ActivityType.Playing);
	}

	public static IActivity? GetActivity(this Phase phase, Config config)
		=> phase switch {
			Phase.Signups => Watching("for signups"),
			Phase.Test => Playing("and testing"),
			Phase.Build => Playing("and building"),
			Phase.Verify => Watching("and verifying"),
			Phase.Open => Playing(config.ConName),
			_ => null
		};

	public static void AddToOption(this Phase phase, SlashCommandOptionBuilder builder) {
		builder.AddChoice(phase.GetName(), (int) phase);
	}
}

public static class Phases {
	public static void AddAllToOption(SlashCommandOptionBuilder builder) {
		foreach (Phase phase in Enum.GetValues(typeof(Phase))) {
			phase.AddToOption(builder);
		}
	}
}
