namespace VirgilAgent.Core;

/// <summary>
/// Options for configuring the OpenAI service.
/// </summary>
public record OpenAIOptions(
	string Endpoint,
	string Key,
	string DeploymentName,
	int? MaxTokens,
	float? Temperature);
