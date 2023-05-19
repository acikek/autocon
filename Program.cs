using Discord;
using Discord.WebSocket;

Task Log(LogMessage message) {
	Console.WriteLine(message.ToString());
	return Task.CompletedTask;
}

var client = new DiscordSocketClient();
var token = Environment.GetEnvironmentVariable("AUTOCON_TOKEN");

client.Log += Log;

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await Task.Delay(-1);
