namespace CniDotNet.Abstractions;

public interface IFilesystem
{
    bool FileOrDirectoryExists(string path);

    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken);
}