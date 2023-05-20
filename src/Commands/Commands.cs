using Discord;
using Models;

namespace Commands;

public class Commands 
{

	public const string ADMIN = "admin";
	public const string PROGRESS = "progress";
	public const string MODAL = "modal";

	public static SlashCommandBuilder GetAdminBuilder(Context context)
		=> new SlashCommandBuilder()
			.WithName(ADMIN)
			.WithDescription("Admin control panel")
			.WithDefaultPermission(false)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(PROGRESS)
				.WithDescription("Progresses the current con phase")
				.WithType(ApplicationCommandOptionType.SubCommand))
			.AddOption(new SlashCommandOptionBuilder()
				.WithName(MODAL)
				.WithDescription("Displays a modal and embeds its results")
				.WithType(ApplicationCommandOptionType.SubCommand)
				.AddOption(new SlashCommandOptionBuilder()
					.WithName("id")
					.WithDescription("The ID of the modal")
					.WithType(ApplicationCommandOptionType.String)
					.WithRequired(true)
					.AddModalChoices(context)));
}
