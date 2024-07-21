﻿using StackExchange.Redis;
using System.Text.Json;

namespace VirgilAgent.Core.Cache;

/// <summary>
/// Redis implementation of the <see cref="ICache"/> interface.
/// </summary>
public class RedisCache(IConnectionMultiplexer redis, int? defaultExpirationInSeconds) : ICache
{
	private readonly int? _defaultExpirationInSeconds = defaultExpirationInSeconds;
	private readonly IDatabase _database = redis.GetDatabase();

	/// <inheritdoc />
	public TValue Get<TValue>(string key, TValue defaultValue)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
		}

		RedisValue value = _database.StringGet(key);
		if (value.IsNull)
		{
			return defaultValue;
		}

		return JsonSerializer.Deserialize<TValue>(value.ToString()) ?? defaultValue;
	}

	/// <inheritdoc />
	public void Set<TValue>(string key, TValue value, TimeSpan? expiresIn = null)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
		}

		if (value is null)
		{
			throw new ArgumentNullException(nameof(value));
		}

		string serializedValue = JsonSerializer.Serialize(value);

		TimeSpan? expiration = expiresIn.HasValue
			? expiresIn.Value
			: _defaultExpirationInSeconds.HasValue
				? TimeSpan.FromSeconds(_defaultExpirationInSeconds.Value)
				: null;

		_database.StringSet(key, serializedValue, expiration);
	}

	/// <inheritdoc />
	public void Remove(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
		}

		_database.KeyDelete(key);
	}

	/// <inheritdoc />
	public void Clear()
	{
		_database.Multiplexer.GetEndPoints()
			.SelectMany(endPoint => _database.Multiplexer.GetServer(endPoint).Keys())
			.ToList()
			.ForEach(key => _database.KeyDelete(key));
	}

	/// <inheritdoc />
	public bool ContainsKey(string key)
	{
		return _database.KeyExists(key);
	}
}
