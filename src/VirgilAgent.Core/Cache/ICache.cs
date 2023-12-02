namespace VirgilAgent.Core.Cache;

public interface ICache
{
	TValue Get<TValue>(string key, TValue defaultValue);
	void Set<TValue>(string key, TValue value, TimeSpan? expiresIn = null);
	bool ContainsKey(string key);
	void Remove(string key);
	void Clear();
}
