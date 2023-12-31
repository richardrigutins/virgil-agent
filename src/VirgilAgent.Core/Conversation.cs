﻿using System.Text.Json.Serialization;

namespace VirgilAgent.Core;

/// <summary>
/// Represents a chat conversation.
/// </summary>
public record Conversation
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = Guid.Empty.ToString();

	[JsonPropertyName("messages")]
	public List<ConversationMessage> Messages { get; set; } = [];
}
