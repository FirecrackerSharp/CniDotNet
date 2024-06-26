namespace CniDotNet.Abstractions;

public sealed class InMemoryCacheBackend : ICacheBackend
{
    private readonly Dictionary<string, object> _backingStore = [];
    
    public bool Exists(string key)
    {
        return _backingStore.ContainsKey(key);
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = _backingStore.GetValueOrDefault(key);
        if (value is T castValue)
        {
            return Task.FromResult<T?>(castValue);
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value) where T : class
    {
        _backingStore[key] = value;
        return Task.CompletedTask;
    }
}