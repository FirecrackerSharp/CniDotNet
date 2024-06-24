using CniDotNet.Data;

namespace CniDotNet.Host;

public interface ICniHost
{
    string GetTempFilePath();

    Task WriteFileAsync(string path, string content, CancellationToken cancellationToken);

    bool DirectoryExists(string path);

    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken);

    void DeleteFile(string path);

    IEnumerable<string> EnumerateDirectory(string path, string searchPattern, SearchOption searchOption);

    Task<ICniHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken);
}