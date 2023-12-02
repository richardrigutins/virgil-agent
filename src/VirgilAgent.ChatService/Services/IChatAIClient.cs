using VirgilAgent.Core;

namespace VirgilAgent.ChatService.Services;

internal interface IChatAIClient
{
	Task<string> GetChatResponseAsync(
	string userInput,
		Conversation currentConversation,
		CancellationToken cancellationToken = default);

	Task<string> GetGreetingsMessageAsync(
		string locale,
		CancellationToken cancellationToken = default);

	Task<string> GetRestartConversationMessageAsync(
		string locale,
		CancellationToken cancellationToken = default);
}
