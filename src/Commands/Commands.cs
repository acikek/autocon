using Discord;

namespace Commands;

public class Commands {

	public const string ADMIN = "admin";
	public const string PROGRESS = "progress";

	public static SlashCommandBuilder ADMIN_BUILDER = new SlashCommandBuilder()
			.WithName(ADMIN)
			.WithDescription("Admin control panel")
			.WithDefaultPermission(false)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(PROGRESS)
				.WithDescription("Progresses the current con phase")
				.WithType(ApplicationCommandOptionType.SubCommand));
}
