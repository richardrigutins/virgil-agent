namespace VirgilAgent.Core;

public record SuggestedAction(
	string Text,
	ActionType ActionType,
	string? ActionData);
