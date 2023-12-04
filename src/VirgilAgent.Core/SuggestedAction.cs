namespace VirgilAgent.Core;

/// <summary>
/// Data structure for suggested actions.
/// </summary>
public record SuggestedAction(
	string Text,
	ActionType ActionType,
	string? ActionData);
