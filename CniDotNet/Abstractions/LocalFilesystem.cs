namespace CniDotNet.Abstractions;

public sealed class LocalFilesystem : IFilesystem
{
    public static LocalFilesystem Current { get; } = new();
    
    private LocalFilesystem() {}
    
    public bool FileOrDirectoryExists(string path)
    {
        return File.Exists(path);
    }

    public async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        return await File.ReadAllTextAsync(path, cancellationToken);
    }
}