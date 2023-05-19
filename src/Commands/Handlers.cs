using Discord;
using Discord.WebSocket;
using Models;
using Structs;

namespace Commands;

public class Handlers {

	public static MessageComponent GetProgressionButtons(bool enabled)
		=> new ComponentBuilder()
			.WithButton(new ButtonBuilder()
				.WithStyle(ButtonStyle.Primary)
				.WithLabel("Progress")
				.WithCustomId("progress_yes")
				.WithDisabled(!enabled))
			.WithButton(new ButtonBuilder()
				.WithStyle(ButtonStyle.Danger)
				.WithLabel("Abort")
				.WithCustomId("progress_no")
				.WithDisabled(!enabled))
			.Build();

	private static string GetProgressionMessage(Phase nextPhase) {
		var message = $"The convention will progress to the **{nextPhase.GetName()}** phase.";
		var warning = nextPhase.GetAdditionalWarning();
		if (warning is not null) {
			message += $"\nAdditionally, {warning}";
		}
		return message + "\n\nAre you sure about this? **You cannot undo this action!**";
	}

	public static async Task HandleAdmin(SocketSlashCommand command, Properties properties) {
		var sub = command.Data.Options.First();

		if (sub.Name == Commands.PROGRESS) {
			if (properties.Phase.IsFinal()) {
				await command.RespondAsync("The convention is already in the final phase!", ephemeral: true);
				return;
			}
			var message = GetProgressionMessage(properties.Phase.GetNext());
			await command.RespondAsync(message, components: GetProgressionButtons(true), ephemeral: true);
		}
	}

	public static async Task HandleProgress(SocketMessageComponent component, Properties properties) {
		string result = component.Data.CustomId.Split("progress_")[1];
		bool progress = result == "yes";

		if (progress)
		{
			properties.ProgressPhase();
			properties.Write();
		}

		await component.UpdateAsync(msg =>
		{
			msg.Content = progress
				? $"The con progressed to the **{properties.Phase.GetName()}** phase successfully."
				: "Progression aborted."; 
			msg.Components = GetProgressionButtons(false);
		});
	}
}
