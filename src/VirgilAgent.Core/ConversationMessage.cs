﻿using System.Text.Json.Serialization;

namespace VirgilAgent.Core;

/// <summary>
/// Represents a single message in a chat conversation.
/// </summary>
public record ConversationMessage
{
	public ConversationMessage(ConversationRole role, string content)
	{
		Role = role;
		Content = content;
	}

	[JsonPropertyName("role")]
	public string RoleLabel
	{
		get => Role.ToString();
		set => Role = Enum.Parse<ConversationRole>(value);
	}

	[JsonIgnore]
	public ConversationRole Role { get; set; }

	[JsonPropertyName("content")]
	public string Content { get; set; } = string.Empty;
}
