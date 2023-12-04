using VirgilAgent.Core;

namespace VirgilAgent.SuggestionsService.Services;

/// <summary>
/// Interface for clients that provide suggestions using AI capabilities.
/// </summary>
internal interface ISuggestionsAIClient
{
	/// <summary>
	/// Gets a list of suggested actions based on the provided input.
	/// </summary>
	/// <param name="input">The input string based on which suggestions are to be made.</param>
	/// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
	/// <returns>A Task that represents the asynchronous operation. The Task's result is a list of suggested actions.</returns>
	Task<List<SuggestedAction>> GetSuggestedActionsAsync(string input, CancellationToken cancellationToken = default);
}
