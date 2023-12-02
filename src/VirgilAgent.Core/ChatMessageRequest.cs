namespace VirgilAgent.Core;

public record ChatMessageRequest(
	string Message,
	string ConversationId);

