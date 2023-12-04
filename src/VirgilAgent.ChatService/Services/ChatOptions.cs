namespace VirgilAgent.ChatService.Services;

/// <summary>
/// Represents the options for configuring the chat service.
/// </summary>
/// <param name="MaxSavedMessages">The maximum number of messages that can be saved.</param>
internal record ChatOptions(short? MaxSavedMessages);
