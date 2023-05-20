using Commands;
using Discord;
using Discord.WebSocket;
using Models;

Task Log(LogMessage message) {
	Console.WriteLine(message.ToString());
	return Task.CompletedTask;
}

var client = new DiscordSocketClient();
var config = Config.Read();
var properties = Properties.Read();
var modals = new Modals();

var context = new Context(client, config, properties, modals);

client.Log += Log;

client.Ready += async () => 
{
	await properties.UpdateActivity(context);
	await Tasks.OnReady(context);
};

client.SlashCommandExecuted += command => Tasks.OnCommand(command, context);
client.ButtonExecuted += component => Tasks.OnButton(component, context);
client.ModalSubmitted += modal => Tasks.OnModal(modal, context);

var token = Environment.GetEnvironmentVariable("AUTOCON_TOKEN");

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await Task.Delay(-1);
