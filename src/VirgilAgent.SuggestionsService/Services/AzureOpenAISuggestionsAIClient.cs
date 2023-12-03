using Azure;
using Azure.AI.OpenAI;
using VirgilAgent.Core;

namespace VirgilAgent.SuggestionsService.Services;

internal class AzureOpenAISuggestionsAIClient : ISuggestionsAIClient
{
	private const string SystemPrompt = @"
You suggest possible follow-up actions and questions to a given input message.
All your suggestion are related to tourism and art. Do not give suggestions on other topics, but reply with an empty response instead.
A follow-up question consists of a string text containing the actual message, an action type, and a string containing additional action data.
The actions can be of three following types:
- Reply: represents a follow-up question; the action data is null in this case.
- Url: represents a URL that gives more information about the subject in the message; the action data contains the actual url.
- Map: represents a location that is present in the message and that can be opened in a map; the action data contains the location to open on the map.

Reply giving a series of suggestions, in the following format:
suggestion 1|action type 1|action data 1
suggestion 2|action type 2|action data 2

Translate the suggessions in the language of the input message, but do not translate the action type.
Give an empty response if you don't have any relevant suggestions to suggest.

Q. Florence, Italy is a city rich in history, art, and culture. There are many attractions to see in Florence. Here are some popular ones: 1. The Duomo (Cathedral of Santa Maria del Fiore) - This iconic dome dominates the city's skyline and offers breathtaking views from the top. 2. Uffizi Gallery - One of the world's most famous art museums, housing masterpieces by renowned artists such as Botticelli, Leonardo da Vinci, and Michelangelo.
A. 
Show map of Florence|Map|Florence
What are some typical foods in Florence?|Reply|

Q. The Uffizi Gallery is one of the most renowned art museums in the world, located in Florence, Italy. It houses an extensive collection of artworks, predominantly from the Italian Renaissance period.
A.
Show in map|Map|Uffizzi Gallery, Florence
Uffizzi Gallery website|Url|https://www.uffizi.it/en

Q. To purchase tickets for the Uffizi Gallery in Florence, Italy, you have a few options: 1. Official Uffizi Gallery website: You can visit the official website of the Uffizi Gallery to buy tickets online. The website provides information about ticket availability, prices, and allows you to select a specific date and time for your visit. Simply go to the Uffizi Gallery's website and follow the instructions to purchase your tickets. 2. Ticket offices at the Uffizi Gallery: If you prefer to purchase tickets in person, you can visit the ticket offices located at the Uffizi Gallery itself. However, it's important to note that during peak tourist seasons, there might be long queues, so it's advisable to arrive early.
A. 
Buy tickets on website|Url|https://www.uffizi.it/en/tickets
";

	private readonly OpenAIClient _openAIClient;
	private readonly AzureOpenAIOptions _options;
	private readonly ILogger<AzureOpenAISuggestionsAIClient> _logger;

	public AzureOpenAISuggestionsAIClient(AzureOpenAIOptions options, ILogger<AzureOpenAISuggestionsAIClient> logger)
	{
		_options = options;

		Uri endpoint = new(_options.Endpoint);
		AzureKeyCredential token = new(_options.Key);

		_openAIClient = new(endpoint, token);
		_logger = logger;
	}

	public async Task<List<SuggestedAction>> GetSuggestedActionsAsync(
		string input,
		CancellationToken cancellationToken = default)
	{
		List<SuggestedAction> result = [];

		ChatCompletionsOptions completionsOptions = CreateCompletionsOptions();

		// Add the user's message to the list of messages.
		ChatMessage userMessage = new(ChatRole.User, input);
		completionsOptions.Messages.Add(userMessage);

		ChatCompletions response = await _openAIClient.GetChatCompletionsAsync(completionsOptions, cancellationToken);

		ChatMessage assistantResponse = response.Choices[0].Message;
		string? responseContent = assistantResponse.Content;

		if (string.IsNullOrWhiteSpace(responseContent))
		{
			_logger.LogInformation("AI didn't return any suggestion");
			return result;
		}

		string[] lines = responseContent.Split(
		  new[] { Environment.NewLine, "\n" },
		  StringSplitOptions.None
		);

		foreach (var line in lines)
		{
			if (TryParseLine(line, out SuggestedAction? suggestedAction) && suggestedAction is not null)
			{
				result.Add(suggestedAction);
			}
			else
			{
				_logger.LogWarning($"AI response couldn't be parsed as {nameof(SuggestedAction)}");
			}
		}

		return result;
	}

	private static bool TryParseLine(string line, out SuggestedAction? suggestedAction)
	{
		if (string.IsNullOrWhiteSpace(line))
		{
			suggestedAction = default;
			return false;
		}

		try
		{
			string[] parts = line.Split('|');
			string text = parts[0];
			ActionType actionType = Enum.Parse<ActionType>(parts[1]);
			string? actionData = parts.Length > 2 ? parts[2] : null;

			suggestedAction = new(text, actionType, actionData);
			return true;
		}
		catch
		{
			suggestedAction = default;
			return false;
		}
	}

	private ChatCompletionsOptions CreateCompletionsOptions()
	{
		List<ChatMessage> messages =
		[
			new ChatMessage(ChatRole.System, SystemPrompt),
		];

		ChatCompletionsOptions completionsOptions = new(_options.DeploymentName, messages)
		{
			MaxTokens = _options.MaxTokens,
			Temperature = _options.Temperature,
		};

		return completionsOptions;
	}
}
