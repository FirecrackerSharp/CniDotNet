using System.Text;
using CniDotNet.Data.Options;

namespace CniDotNet.Abstractions;

public interface IRuntimeHost
{
    string GetTempFilePath();

    Task WriteFileAsync(string path, string content, CancellationToken cancellationToken);

    bool DirectoryExists(string path);

    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken);

    Task DeleteFileAsync(string path, CancellationToken cancellationToken);

    Task<IEnumerable<string>> EnumerateDirectoryAsync(string path, string searchPattern, SearchOption searchOption,
        CancellationToken cancellationToken);

    Task<string?> GetEnvironmentVariableAsync(string variableName, CancellationToken cancellationToken);

    Task<IRuntimeHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken);
    
    public static string BuildEnvironmentString(Dictionary<string, string> environment)
    {
        var environmentBuilder = new StringBuilder();

        foreach (var (key, value) in environment)
        {
            environmentBuilder.Append($"{key}={value} ");
        }

        return environmentBuilder.ToString().TrimEnd();
    }
}