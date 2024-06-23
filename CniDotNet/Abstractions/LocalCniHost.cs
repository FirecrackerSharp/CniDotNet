namespace CniDotNet.Abstractions;

public sealed class LocalCniHost : ICniHost
{
    public static LocalCniHost Current { get; } = new();
    
    private LocalCniHost() {}
    
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public IEnumerable<string> EnumerateDirectory(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    public ICniHostProcess StartProcess(string executable, string args, Dictionary<string, string> environment,
        string? elevationPassword)
    {
        return null;
    }
}