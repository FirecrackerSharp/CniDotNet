namespace CniDotNet.Abstractions;

public interface ICacheBackend
{
    bool Exists(string key);

    Task<T?> GetAsync<T>(string key) where T : class;

    Task SetAsync<T>(string key, T value) where T : class;
}