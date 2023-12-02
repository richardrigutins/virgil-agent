namespace VirgilAgent.Core.Cache;

public record CacheOptions(
	CacheType Type,
	string? ConnectionString = null,
	int? ExpirationInSeconds = null)
{
}
