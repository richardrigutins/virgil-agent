using Azure;
using Azure.AI.OpenAI;
using VirgilAgent.Core;

namespace VirgilAgent.ChatService.Services;

/// <summary>
/// OpenAI implementation of the <see cref="IChatAIClient"/> interface.
/// </summary>
internal class OpenAIChatAIClient : IChatAIClient
{
	private const string SystemPrompt = @"
Your name is Virgil.
You are a helpful tourist guide who assists people by providing information about the city they are visiting and helping them find places to eat and stay.
You answer questions about cities, places to visit, history, art, culture, places to eat, places to stay, and other topics related to tourism.
You also give suggestions about places to visit, when to visit them, and how to get there.
You are friendly and polite.
Give answers that are less than 200 words long.
Don't answer questions on topics that don't relate to tourism.
Never translate your name to other languages, it should always remain Virgil 
(e.g. ""Mi chiamo Virgil"", ""Je m'appelle Virgil"", ""Me llamo Virgil"", etc.).
Reply that you don't know if you don't know how to answer a question.";

	private readonly OpenAIClient _openAIClient;
	private readonly OpenAIOptions _options;

	public OpenAIChatAIClient(OpenAIOptions options)
	{
		_options = options;

		if (string.IsNullOrWhiteSpace(_options.Endpoint))
		{
			// Use a non-Azure OpenAI endpoint.
			_openAIClient = new(_options.Key);
		}
		else
		{
			// Use the specified Azure OpenAI Service endpoint.
			Uri endpoint = new(_options.Endpoint);
			AzureKeyCredential token = new(_options.Key);

			_openAIClient = new(endpoint, token);
		}
	}

	/// <inheritdoc/>
	public async Task<string> GetChatResponseAsync(
		string userInput,
		Conversation currentConversation,
		CancellationToken cancellationToken = default)
	{
		List<ConversationMessage> conversationMessages = currentConversation.Messages;

		// Convert the list of conversation messages to a list of ChatMessage.
		var chatMessages = conversationMessages.Select(ToChatMessage).ToList();

		// Create a completions options object with all the previous messages.
		ChatCompletionsOptions completionsOptions = CreateCompletionsOptions(chatMessages);

		// Add the user's message to the list of messages.
		ChatMessage userMessage = new(ChatRole.User, userInput);
		completionsOptions.Messages.Add(userMessage);

		ChatCompletions response = await _openAIClient.GetChatCompletionsAsync(completionsOptions, cancellationToken);

		ChatMessage assistantResponse = response.Choices[0].Message;

		return assistantResponse.Content;
	}

	/// <inheritdoc/>
	public async Task<string> GetGreetingsMessageAsync(
		string locale,
		CancellationToken cancellationToken = default)
	{
		// Create a completions options object with only the system prompt.
		ChatCompletionsOptions tempOptions = CreateCompletionsOptions();
		tempOptions.Messages.Add(
			new ChatMessage(
				ChatRole.User,
				$"Hello! Briefly introduce yourself in a couple of sentences. Reply in the language of the following locale: {locale}."
				));

		ChatCompletions response = await _openAIClient.GetChatCompletionsAsync(tempOptions, cancellationToken);

		ChatMessage assistantResponse = response.Choices[0].Message;

		return assistantResponse.Content;
	}

	/// <inheritdoc/>
	public async Task<string> GetRestartConversationMessageAsync(
		string locale,
		CancellationToken cancellationToken = default)
	{
		// Create a completions options object with only the system prompt.
		ChatCompletionsOptions tempOptions = CreateCompletionsOptions();
		tempOptions.Messages.Add(
			new ChatMessage(
				ChatRole.User,
				$"Let's start a new conversation. Reply in the language of the following locale: {locale}."
				));

		ChatCompletions response = await _openAIClient.GetChatCompletionsAsync(tempOptions, cancellationToken);

		ChatMessage assistantResponse = response.Choices[0].Message;

		return assistantResponse.Content;
	}

	/// <summary>
	/// Creates a completions options object with the system prompt and optionally the previous messages.
	/// </summary>
	/// <param name="previousMessages">The previous messages in the conversation.</param>
	/// <returns>A reference to the created completions options object.</returns>
	private ChatCompletionsOptions CreateCompletionsOptions(List<ChatMessage>? previousMessages = null)
	{
		List<ChatMessage> messages =
		[
			new ChatMessage(ChatRole.System, SystemPrompt),
		];

		if (previousMessages is not null)
		{
			messages.AddRange(previousMessages);
		}

		ChatCompletionsOptions completionsOptions = new(_options.DeploymentName, messages)
		{
			MaxTokens = _options.MaxTokens,
			Temperature = _options.Temperature,
		};

		return completionsOptions;
	}

	/// <summary>
	/// Converts a <see cref="ConversationMessage"/> to a <see cref="ChatMessage"/>.
	/// </summary>
	/// <param name="conversationMessage">The conversation message to convert.</param>
	/// <returns>The converted chat message.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the conversation role is invalid.</exception>
	private ChatMessage ToChatMessage(ConversationMessage conversationMessage)
	{
		ChatRole chatRole = conversationMessage.Role switch
		{
			ConversationRole.User => ChatRole.User,
			ConversationRole.Assistant => ChatRole.Assistant,
			_ => throw new InvalidOperationException($"Invalid conversation role: {conversationMessage.Role}"),
		};

		return new ChatMessage(chatRole, conversationMessage.Content);
	}

	/// <summary>
	/// Converts a <see cref="ChatMessage"/> to a <see cref="ConversationMessage"/>.
	/// </summary>
	/// <param name="chatMessage">The chat message to convert.</param>
	/// <returns>The converted conversation message.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the chat role is invalid.</exception>
	private ConversationMessage ToConversationMessage(ChatMessage chatMessage)
	{
		ConversationRole conversationRole;
		if (chatMessage.Role == ChatRole.User)
		{
			conversationRole = ConversationRole.User;
		}
		else if (chatMessage.Role == ChatRole.Assistant)
		{
			conversationRole = ConversationRole.Assistant;
		}
		else
		{
			throw new InvalidOperationException($"Invalid chat role: {chatMessage.Role}");
		}

		return new ConversationMessage(conversationRole, chatMessage.Content);
	}
}
