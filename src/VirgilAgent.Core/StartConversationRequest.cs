namespace VirgilAgent.Core;

/// <summary>
/// Data structure for sending a request to start a conversation.
/// </summary>
public record StartConversationRequest(
	string? Locale,
	string? ConversationId);
