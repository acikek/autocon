using Discord;
using Discord.WebSocket;

namespace Commands.Handlers;

public static class General
{
	public const string FORM_CHOICE_BOOTH = "booth";
	public const string FORM_CHOICE_EVENT = "event";

	public const string SELECT_BUILDERS = "builders";
	public const string BUILDER_CHOICE_USER = "user";
	public const string BUILDER_CHOICE_ORG = "org";

	private static MessageComponent GetBuilderSelectMenu()
		=> new ComponentBuilder()
			.WithSelectMenu(new SelectMenuBuilder()
				.WithCustomId(SELECT_BUILDERS)
				.WithPlaceholder("Who should build this booth?")
				.AddOption(new SelectMenuOptionBuilder()
					.WithLabel("Myself")
					.WithValue(BUILDER_CHOICE_USER)
					.WithDescription("You'll build the booth")
					.WithEmote(new Emoji("\uD83C\uDF1F")))
				.AddOption(new SelectMenuOptionBuilder()
					.WithLabel("Organizers")
					.WithValue(BUILDER_CHOICE_ORG)
					.WithDescription("We'll build it for you")
					.WithEmote(new Emoji("\uD83D\uDC77"))))
			.Build();

	public static async Task HandleFormSelection(SocketMessageComponent component, Context context)
	{
		switch (component.Data.Values.First())
		{
			case FORM_CHOICE_BOOTH:
				await component.RespondAsync(components: GetBuilderSelectMenu());
				break;
		}
		//await Modals.FromId(component.Data.Values.First()).Display(component, ModalPurpose.Form);
	}

	public static async Task HandlePostForm(SocketModal modal, string id, Context context)
	{
		switch (id)
		{
			case FORM_CHOICE_BOOTH:
				break;
		}
	}
}
