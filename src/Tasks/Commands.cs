using Database;
using Discord.WebSocket;
using Models;
using Structs;

namespace Tasks;

public class Commands {

	public const string ADMIN = "admin";
	public const string ADMIN_PROGRESS = "progress";

	public static async Task OnCommand(SocketSlashCommand command, Properties properties) {
		switch (command.Data.Name) {
			case ADMIN: 
				await HandleAdmin(command, properties);
				break;
		}
	}

	public static Phase? changingPhase = null;

	public static async Task HandleAdmin(SocketSlashCommand command, Properties properties) {
		var sub = command.Data.Options.First();
		if (sub.Name == ADMIN_PROGRESS) {
			var phase = (Phase) (long) sub.Options.First().Value;
			await command.RespondAsync(phase.GetName());
			properties.Phase = phase;
			properties.Write();
		}
	}
}