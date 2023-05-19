using Discord.WebSocket;

namespace Tasks;

public class Commands {

	public const string ADMIN = "admin";
	public const string ADMIN_PROGRESS = "progress";

	public static async Task OnCommand(SocketSlashCommand command) {
		switch (command.Data.Name) {
			case ADMIN: 
				await HandleAdmin(command);
				break;
		}
	}

	public static Phase? changingPhase = null;

	public static async Task HandleAdmin(SocketSlashCommand command) {
		var sub = command.Data.Options.First();
		if (sub.Name == ADMIN_PROGRESS) {
			var phaseId = (int) sub.Options.First().Value;
			var phase = (Phase) phaseId;
			await command.RespondAsync(phase.GetName());
		}
		//Console.WriteLine(command.);
		//var sub = (S) command.Data.Options.First();
		/*Console.WriteLine(options);
		if (options.Count() < 2) {
			return;
		}
		var sub = options.ElementAt(0);
		if (sub.Name == ADMIN_PROGRESS) {*/
		/*Console.WriteLine(options.First().Value);
*/
		//}
	}
}