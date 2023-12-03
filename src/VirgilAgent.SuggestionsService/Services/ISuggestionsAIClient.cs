using VirgilAgent.Core;

namespace VirgilAgent.SuggestionsService.Services;

internal interface ISuggestionsAIClient
{
	Task<List<SuggestedAction>> GetSuggestedActionsAsync(string input, CancellationToken cancellationToken = default);
}
