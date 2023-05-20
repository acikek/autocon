using Discord;
using Discord.WebSocket;
using Models;
using Structs;

namespace Commands;

public class Handlers 
{

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

	private static string GetProgressionMessage(Phase nextPhase) 
	{
		var message = $"The convention will progress to the **{nextPhase.GetName()}** phase.";
		var warning = nextPhase.GetAdditionalWarning();
		if (warning is not null) 
		{
			message += $"\nAdditionally, {warning}";
		}
		return message + "\n\nAre you sure about this? **You cannot undo this action!**";
	}

	public static async Task HandleAdmin(SocketSlashCommand command, Context context) 
	{
		var sub = command.Data.Options.First();

		switch (sub.Name) 
		{
			case Commands.PROGRESS:
				if (context.Properties.Phase.IsFinal()) 
				{
					await command.RespondAsync("The convention is already in the final phase!", ephemeral: true);
					return;
				}
				var message = GetProgressionMessage(context.Properties.Phase.GetNext());
				await command.RespondAsync(message, components: GetProgressionButtons(true), ephemeral: true);
				break;
			case Commands.MODAL:
				var modalId = (string) sub.Options.First().Value;
				await context.Modals[modalId].Display(command, ModalPurpose.Testing);
				break;
		}
	}

	public static async Task HandleProgress(SocketMessageComponent component, Context context) 
	{
		string result = component.Data.CustomId.Split("progress_")[1];
		bool progress = result == "yes";

		if (progress)
		{
			await context.Properties.ProgressPhase(context);
			context.Properties.Write();
		}

		await component.UpdateAsync(msg =>
		{
			msg.Content = progress
				? $"The con progressed to the **{context.Properties.Phase.GetName()}** phase successfully."
				: "Progression aborted."; 
			msg.Components = GetProgressionButtons(false);
		});
	}
}
