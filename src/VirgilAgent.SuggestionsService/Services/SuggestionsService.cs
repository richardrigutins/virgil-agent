using CommunityToolkit.Diagnostics;
using VirgilAgent.Core;

namespace VirgilAgent.SuggestionsService.Services;

internal class SuggestionsService(ISuggestionsAIClient suggestionsAIClient)
{
	private readonly ISuggestionsAIClient _suggestionsAIClient = suggestionsAIClient;

	public async Task<List<SuggestedAction>> GetSuggestedActionsAsync(
		string message,
		CancellationToken cancellationToken = default)
	{
		Guard.IsNotNullOrWhiteSpace(message);

		return await _suggestionsAIClient.GetSuggestedActionsAsync(message, cancellationToken);
	}
}
