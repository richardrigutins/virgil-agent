namespace VirgilAgent.Core;

/// <summary>
/// Data structure for sending a chat message request.
/// </summary>
public record ChatMessageRequest(
	string Message,
	string ConversationId);
