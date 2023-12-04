namespace VirgilAgent.Core;

/// <summary>
/// Data structure for chat message responses.
/// </summary>
public record ChatMessageResponse(
	string Message,
	string ConversationId);
