using System.Net.Mime;
using System.Text;
using System.Text.Json;
using VirgilAgent.Core;

namespace VirgilAgent.BotService.Services;

internal class ChatApiClient(HttpClient httpClient)
{
	private readonly HttpClient _httpClient = httpClient;

	public async Task<ChatMessageResponse> StartConversationAsync(StartConversationRequest? request)
	{
		string jsonBody = JsonSerializer.Serialize(request);
		using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json);

		HttpResponseMessage response = await _httpClient.PostAsync("/start", content);

		if (!response.IsSuccessStatusCode)
		{
			string body = await response.Content.ReadAsStringAsync();
			throw new ApiException(body, null, response.StatusCode);
		}

		ChatMessageResponse? result = await response.Content.ReadFromJsonAsync<ChatMessageResponse>()
			?? throw new ApiException($"Couldn't read response as {nameof(ChatMessageResponse)}");

		return result;
	}

	public async Task<ChatMessageResponse> SendMessageAsync(ChatMessageRequest request)
	{
		string jsonBody = JsonSerializer.Serialize(request);
		using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json);

		HttpResponseMessage response = await _httpClient.PostAsync("/chat", content);

		if (!response.IsSuccessStatusCode)
		{
			string body = await response.Content.ReadAsStringAsync();
			throw new ApiException(body, null, response.StatusCode);
		}

		ChatMessageResponse? result = await response.Content.ReadFromJsonAsync<ChatMessageResponse>()
			?? throw new ApiException($"Couldn't read response as {nameof(ChatMessageResponse)}");

		return result;
	}
}
