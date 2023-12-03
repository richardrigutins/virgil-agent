using System.Net;

namespace VirgilAgent.BotService.Services;

internal class ApiException(string? message, Exception? innerException = null, HttpStatusCode? statusCode = null)
	: Exception(message, innerException)
{
	public HttpStatusCode? StatusCode { get; set; } = statusCode;
}
