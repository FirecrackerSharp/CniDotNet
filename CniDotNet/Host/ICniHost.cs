namespace CniDotNet.Host;

public interface ICniHost
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken);

    IEnumerable<string> EnumerateDirectory(string path, string searchPattern, SearchOption searchOption);

    bool IsRoot { get; }
    
    Task<ICniHostProcess> StartProcessWithElevationAsync(string command, Dictionary<string, string> environment,
        string elevationPassword, string sudoPath, CancellationToken cancellationToken);
}