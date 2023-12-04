using CommunityToolkit.Diagnostics;
using VirgilAgent.Core;

namespace VirgilAgent.SuggestionsService.Services;

/// <summary>
/// Service for handling suggestions.
/// </summary>
internal class SuggestionsService(ISuggestionsAIClient suggestionsAIClient)
{
	private readonly ISuggestionsAIClient _suggestionsAIClient = suggestionsAIClient;

	/// <summary>
	/// Gets a list of suggested actions based on the provided message.
	/// </summary>
	/// <param name="message">The message based on which suggestions are to be made. It must not be empty or whitespace.</param>
	/// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
	/// <returns>A Task that represents the asynchronous operation. The Task's result is a list of suggested actions.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty or whitespace.</exception>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
	public async Task<List<SuggestedAction>> GetSuggestedActionsAsync(
		string message,
		CancellationToken cancellationToken = default)
	{
		Guard.IsNotNullOrWhiteSpace(message);

		return await _suggestionsAIClient.GetSuggestedActionsAsync(message, cancellationToken);
	}
}
