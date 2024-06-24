namespace CniDotNet.Host;

public interface ICniHost
{
    string GetTempFilePath();

    Task WriteFileAsync(string path, string content, CancellationToken cancellationToken);

    bool DirectoryExists(string path);

    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken);

    IEnumerable<string> EnumerateDirectory(string path, string searchPattern, SearchOption searchOption);

    Task<ICniHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        string elevationPassword, string suPath, CancellationToken cancellationToken);
}