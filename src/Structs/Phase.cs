using Discord;

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

	private static IActivity Watching(string status)
		=> new Game(status, ActivityType.Watching);

	private static IActivity Playing(string status)
		=> new Game(status, ActivityType.Playing);

	public static IActivity? GetActivity(this Phase phase, Context context)
		=> phase switch {
			Phase.Signups => Watching("for signups"),
			Phase.Test => Playing("and testing"),
			Phase.Build => Playing("and building"),
			Phase.Verify => Watching("and verifying"),
			Phase.Open => Playing(context.Config.ConName),
			_ => null
		};

	public static string? GetAdditionalWarning(this Phase phase)
		=> phase switch {
			Phase.Signups => "a selection menu leading to a submission form will be sent in **this channel**.",
			Phase.Build => "submissions will be **closed off permanently**.",
			Phase.Open => "**this will open the convention channels to everyone!**",
			_ => null
		};

	public static Phase GetNext(this Phase phase)
		=> phase + 1;

	public static bool IsFinal(this Phase phase)
		=> phase == Phase.Open;
}
