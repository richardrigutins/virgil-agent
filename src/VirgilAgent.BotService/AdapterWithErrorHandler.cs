using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;

namespace VirgilAgent.BotService;

public class AdapterWithErrorHandler : CloudAdapter
{
	public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<IBotFrameworkHttpAdapter> logger)
		: base(auth, logger)
	{
		OnTurnError = async (turnContext, exception) =>
		{
			// Log any leaked exception from the application.
			logger.LogError(exception, "[OnTurnError] unhandled error : {exceptionMessage}", exception.Message);

			// Send a message to the user
			await turnContext.SendActivityAsync("Something went wrong, please try again later.");

			// Send a trace activity, which will be displayed in the Bot Framework Emulator
			await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
		};
	}
}
