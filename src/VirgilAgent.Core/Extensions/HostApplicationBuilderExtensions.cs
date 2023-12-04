using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VirgilAgent.Core.Cache;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class HostApplicationBuilderExtensions
{
	/// <summary>
	/// Adds the cache to the application.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="sectionName">Name of the section where the cache configuration is located in the configuration file.</param>
	/// <returns>A reference to this instance after the operation has completed.</returns>
	public static IHostApplicationBuilder AddCache(
		this IHostApplicationBuilder builder,
		string sectionName)
	{
		CacheOptions cacheOptions = builder.Configuration.GetSection(sectionName).Get<CacheOptions>()
			?? throw new ArgumentException($"Invalid configuration section: {sectionName}");

		switch (cacheOptions.Type)
		{
			case CacheType.InMemory:
				builder.Services.AddSingleton<ICache>(new InMemoryCache(cacheOptions.ExpirationInSeconds));
				break;
			case CacheType.Redis:
				builder.AddRedis(cacheOptions.ConnectionString ?? string.Empty);
				builder.Services.AddSingleton<ICache>((services) =>
				{
					IConnectionMultiplexer redis = services.GetRequiredService<IConnectionMultiplexer>();
					return new RedisCache(redis, cacheOptions.ExpirationInSeconds);
				});
				break;
			default:
				throw new ArgumentOutOfRangeException(null, cacheOptions.Type, null);
		}

		return builder;
	}
}

