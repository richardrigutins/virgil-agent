namespace VirgilAgent.Core;

/// <summary>
/// Options for configuring the Azure OpenAI service.
/// </summary>
public record AzureOpenAIOptions(
	string Endpoint,
	string Key,
	string DeploymentName,
	int? MaxTokens,
	float? Temperature);
