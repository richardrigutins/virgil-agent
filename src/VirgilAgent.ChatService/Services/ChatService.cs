using CommunityToolkit.Diagnostics;
using VirgilAgent.Core;
using VirgilAgent.Core.Cache;

namespace VirgilAgent.ChatService.Services;

internal class ChatService(ICache cache, IChatAIClient chatClient, ILogger<ChatService> logger)
{
	public const short MaxConversationLength = 20;

	private readonly ICache _cache = cache;
	private readonly IChatAIClient _chatClient = chatClient;
	private readonly ILogger<ChatService> _logger = logger;

	public Conversation BuildEmptyConversation(string? conversationId)
	{
		return new Conversation()
		{
			Id = conversationId ?? Guid.NewGuid().ToString(),
			Messages = [],
		};
	}

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

		// Add the message to the conversation.
		ConversationMessage conversationMessage = new(ConversationRole.Assistant, responseMessage);
		conversation.Messages.Add(conversationMessage);

		// Store the conversation in the cache.
		_cache.Set(conversation.Id, conversation);

		return conversation;
	}

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

	private void AddMessageToConversation(Conversation conversation, ConversationMessage message)
	{
		conversation.Messages.Add(message);

		if (conversation.Messages.Count >= MaxConversationLength)
		{
			_logger.LogInformation(
				"Conversation with id {conversationId} has reached the max length ({maxLength}), the oldest message will be removed",
				conversation.Id,
				MaxConversationLength);

			conversation.Messages.RemoveAt(0);
		}
	}
}
