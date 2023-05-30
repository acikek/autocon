using Commands;
using Discord;
using Discord.WebSocket;
using Forms;
using Structs;

Task Log(LogMessage message) {
	Console.WriteLine(message.ToString());
	return Task.CompletedTask;
}

var client = new DiscordSocketClient();
var config = Config.Read();
var properties = Properties.Read();
var forms = new FormManager();

var context = new BotContext(client, config, properties, forms);

client.Log += Log;

client.Ready += async () => 
{
	await properties.UpdateActivity(context);
	await Tasks.OnReady(context);
};

client.SlashCommandExecuted += command => Tasks.OnCommand(command, context);
client.ButtonExecuted += component => Tasks.OnButton(component, context);
client.ModalSubmitted += modal => Tasks.OnModal(modal, context);
client.SelectMenuExecuted += menu => Tasks.OnSelectMenu(menu, context);

FormInteractions.Register(context);

var token = Environment.GetEnvironmentVariable("AUTOCON_TOKEN");

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await Task.Delay(-1);
