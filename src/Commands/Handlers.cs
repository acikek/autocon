using Discord.WebSocket;
using Models;
using Structs;

namespace Commands;

public class Handlers {

	public static async Task HandleAdmin(SocketSlashCommand command, Properties properties) {
		var sub = command.Data.Options.First();
		if (sub.Name == Commands.PROGRESS) {
			var phase = (Phase) (long) sub.Options.First().Value;
			await command.RespondAsync(phase.GetName());
			properties.Phase = phase;
			properties.Write();
		}
	}
}
