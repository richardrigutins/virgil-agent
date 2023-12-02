namespace VirgilAgent.Core;

public record AzureOpenAIOptions(
	string Endpoint,
	string Key,
	string DeploymentName,
	int? MaxTokens,
	float? Temperature)
{
	public const string SectionName = "AzureOpenAI";
}
