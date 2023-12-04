namespace VirgilAgent.Core.Cache;

/// <summary>
/// Interface for a generic cache.
/// </summary>
public interface ICache
{
	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="defaultValue">The default value to return if the key does not exist.</param>
	/// <returns>The value associated with the specified key, or the default value if the key does not exist.</returns>
	TValue Get<TValue>(string key, TValue defaultValue);

	/// <summary>
	/// Sets the value for the specified key.
	/// </summary>
	/// <param name="key">The key of the value to set.</param>
	/// <param name="value">The value to set.</param>
	/// <param name="expiresIn">The time period after which the key-value pair should expire.</param>
	void Set<TValue>(string key, TValue value, TimeSpan? expiresIn = null);

	/// <summary>
	/// Determines whether the cache contains an entry with the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the cache.</param>
	/// <returns>true if the cache contains an entry with the key; otherwise, false.</returns>
	bool ContainsKey(string key);

	/// <summary>
	/// Removes the value with the specified key from the cache.
	/// </summary>
	/// <param name="key">The key of the value to remove.</param>
	void Remove(string key);

	/// <summary>
	/// Removes all keys and values from the cache.
	/// </summary>
	void Clear();
}
