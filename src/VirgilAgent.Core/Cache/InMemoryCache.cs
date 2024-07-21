namespace VirgilAgent.Core.Cache;

/// <summary>
/// In-memory implementation of the <see cref="ICache"/> interface.
/// </summary>
public class InMemoryCache(int? defaultExpirationInSeconds = null) : ICache
{
	private readonly int? _defaultExpirationInSeconds = defaultExpirationInSeconds;
	private readonly Dictionary<string, (object Value, DateTime? Expiration)> _cache = [];

	/// <inheritdoc />
	public TValue Get<TValue>(string key, TValue defaultValue)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
		}

		if (_cache.TryGetValue(key, out var value))
		{
			if (value.Expiration.HasValue && value.Expiration < DateTime.Now)
			{
				_cache.Remove(key);
				return defaultValue;
			}

			if (value.Value is not TValue)
			{
				throw new InvalidCastException($"Value of type {value.Value.GetType()} cannot be cast to {typeof(TValue)}");
			}

			return (TValue)value.Value;
		}

		return defaultValue;
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

		DateTime? expiration = expiresIn.HasValue
			? DateTime.Now.Add(expiresIn.Value)
			: _defaultExpirationInSeconds.HasValue
				? DateTime.Now.AddSeconds(_defaultExpirationInSeconds.Value)
				: null;

		_cache[key] = (value, expiration);
	}

	/// <inheritdoc />
	public bool ContainsKey(string key)
	{
		return _cache.TryGetValue(key, out var value) && !(value.Expiration.HasValue && value.Expiration < DateTime.Now);
	}

	/// <inheritdoc />
	public void Remove(string key)
	{
		_cache.Remove(key);
	}

	/// <inheritdoc />
	public void Clear()
	{
		_cache.Clear();
	}
}
