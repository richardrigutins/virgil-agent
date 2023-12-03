using VirgilAgent.Core;

namespace VirgilAgent.BotService.Services;

internal class SuggestionsApiClient(HttpClient httpClient)
{
	private readonly HttpClient _httpClient = httpClient;

	public async Task<IEnumerable<SuggestedAction>> GetSuggestedActionsAsync(string message)
	{
		// Encode the message.
		string encodedMessage = Uri.EscapeDataString(message);
		string url = $"/suggestions?message={encodedMessage}";

		HttpResponseMessage response = await _httpClient.GetAsync(url);

		if (!response.IsSuccessStatusCode)
		{
			string body = await response.Content.ReadAsStringAsync();
			throw new ApiException(body, null, response.StatusCode);
		}

		IEnumerable<SuggestedAction>? result = await response.Content.ReadFromJsonAsync<IEnumerable<SuggestedAction>>()
			?? throw new ApiException($"Couldn't read response as {nameof(IEnumerable<SuggestedAction>)}");

		return result;
	}
}
