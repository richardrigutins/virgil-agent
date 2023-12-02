using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VirgilAgent.Core.Cache;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
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

