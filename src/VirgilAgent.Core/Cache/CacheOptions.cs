namespace VirgilAgent.Core.Cache;

/// <summary>
/// Options for configuring the cache behavior.
/// </summary>
public record CacheOptions(
	CacheType Type,
	string? ConnectionString = null,
	int? ExpirationInSeconds = null);
