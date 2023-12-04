using CommunityToolkit.Diagnostics;
using VirgilAgent.Core;
using VirgilAgent.Core.Cache;

namespace VirgilAgent.ChatService.Services;

/// <summary>
/// Service for managing chat conversations.
/// </summary>
internal class ChatService(
	ICache cache,
	IChatAIClient chatClient,
	ILogger<ChatService> logger,
	ChatOptions chatOptions)
{
	private readonly ICache _cache = cache;
	private readonly IChatAIClient _chatClient = chatClient;
	private readonly ILogger<ChatService> _logger = logger;
	private readonly ChatOptions _chatOptions = chatOptions;

	/// <summary>
	/// Starts a new conversation asynchronously. The conversation will start in the specified locale, and will optionally have the specified id.
	/// </summary>
	/// <param name="conversationId">The id of the conversation.</param>
	/// <param name="locale">The locale for the conversation.</param>
	/// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the conversation.</returns>
	public async Task<Conversation> StartConversationAsync(
		string? conversationId = null,
		string? locale = "en",
		CancellationToken cancellationToken = default)
	{
		string selectedLocale = string.IsNullOrWhiteSpace(locale) ? "en" : locale;

		// Call the AI client to get the initial message.
		string responseMessage =
			await _chatClient.GetGreetingsMessageAsync(selectedLocale, cancellationToken);

		// Create a new conversation.
		Conversation conversation = BuildEmptyConversation(conversationId);

		// Clear previous messages with the same conversation id.
		_cache.Remove(conversation.Id);

		// Add the message to the conversation.
		ConversationMessage conversationMessage = new(ConversationRole.Assistant, responseMessage);
		conversation.Messages.Add(conversationMessage);

		// Store the conversation in the cache.
		_cache.Set(conversation.Id, conversation);

		return conversation;
	}

	/// <summary>
	/// Gets a chat response asynchronously. The response is based on the user's input and the previous messages in the conversation.
	/// </summary>
	/// <param name="userInput">The user's input message.</param>
	/// <param name="conversationId">The id of the conversation.</param>
	/// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the chat response.</returns>
	/// <exception cref="ArgumentException">Thrown when the user input or the conversation id are empty or whitespace.</exception>
	/// <exception cref="ArgumentNullException">Thrown when the user input or the conversation id are null.</exception>
	public async Task<string> GetChatResponseAsync(
		string userInput,
		string conversationId,
		CancellationToken cancellationToken = default)
	{
		Guard.IsNotNullOrWhiteSpace(userInput);
		Guard.IsNotNullOrWhiteSpace(conversationId);

		// Get the current conversation or create a new one.
		Conversation? conversation = _cache.Get<Conversation?>(conversationId, null);
		if (conversation is null)
		{
			_logger.LogInformation(
				"Conversation with id {conversationId} not found in cache, it will be initialized",
				conversationId);
			conversation = BuildEmptyConversation(conversationId);
		}

		// Add the user message to the conversation.
		ConversationMessage userMessage = new(ConversationRole.User, userInput);
		AddMessageToConversation(conversation, userMessage);

		// Call the AI client to process the message.
		string responseMessage = await _chatClient.GetChatResponseAsync(userInput, conversation, cancellationToken);

		// Add the assistant message to the conversation.
		ConversationMessage assistantMessage = new(ConversationRole.Assistant, responseMessage);
		AddMessageToConversation(conversation, assistantMessage);

		// Store the updated conversation in the cache.
		_cache.Set(conversation.Id, conversation);

		return responseMessage;
	}

	/// <summary>
	/// Builds an empty conversation with the specified id.
	/// </summary>
	/// <param name="conversationId">The id of the conversation.</param>
	/// <returns>The created conversation.</returns>
	private Conversation BuildEmptyConversation(string? conversationId)
	{
		return new Conversation()
		{
			Id = conversationId ?? Guid.NewGuid().ToString(),
			Messages = [],
		};
	}

	/// <summary>
	/// Adds a message to the conversation. If the conversation has reached the max length, the oldest message will be removed.
	/// </summary>
	private void AddMessageToConversation(Conversation conversation, ConversationMessage message)
	{
		conversation.Messages.Add(message);

		if (conversation.Messages.Count > _chatOptions.MaxSavedMessages)
		{
			_logger.LogInformation(
				"Conversation with id {conversationId} has reached the max length ({maxLength}), the oldest message will be removed",
				conversation.Id,
				_chatOptions.MaxSavedMessages);

			conversation.Messages.RemoveAt(0);
		}
	}
}
