namespace CniDotNet.Abstractions;

public interface ICniHost
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken);

    IEnumerable<string> EnumerateDirectory(string path, string searchPattern, SearchOption searchOption);

    ICniHostProcess StartProcess(string executable, string args, Dictionary<string, string> environment, string? elevationPassword);
}