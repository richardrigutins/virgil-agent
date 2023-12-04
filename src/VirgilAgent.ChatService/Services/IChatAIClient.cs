using VirgilAgent.Core;

namespace VirgilAgent.ChatService.Services;

/// <summary>
/// Interface for clients that communicate with the AI chat service.
/// </summary>
internal interface IChatAIClient
{
	/// <summary>
	/// Gets a chat response to the user's input.
	/// </summary>
	/// <param name="userInput">The user's input message.</param>
	/// <param name="currentConversation">The current conversation, containing the previous messages.</param>
	/// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the chat response.</returns>
	Task<string> GetChatResponseAsync(
		string userInput,
		Conversation currentConversation,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the greetings message asynchronously. The message is localized to the specified locale.
	/// </summary>
	/// <param name="locale">The locale for the greetings message.</param>
	/// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the greetings message.</returns>
	Task<string> GetGreetingsMessageAsync(
		string locale,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the restart conversation message asynchronously. The message is localized to the specified locale.
	/// </summary>
	/// <param name="locale">The locale for the restart conversation message.</param>
	/// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the restart conversation message.</returns> 
	Task<string> GetRestartConversationMessageAsync(
		string locale,
		CancellationToken cancellationToken = default);
}
