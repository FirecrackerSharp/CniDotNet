using System.Diagnostics;
using System.Text;

namespace CniDotNet.Host;

public sealed class LocalCniHost : ICniHost
{
    public static LocalCniHost Current { get; } = new();
    
    private LocalCniHost() {}
    
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string GetTempFilePath()
    {
        return Path.GetTempFileName();
    }

    public Task WriteFileAsync(string path, string content, CancellationToken cancellationToken)
    {
        return File.WriteAllTextAsync(path, content, cancellationToken);
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

    public bool IsRoot => Environment.UserName == "root";

    public async Task<ICniHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        string elevationPassword, string suPath, CancellationToken cancellationToken)
    {
        var environmentBuilder = new StringBuilder();

        foreach (var (key, value) in environment)
        {
            environmentBuilder.Append($"{key}={value} ");
        }

        var environmentString = environmentBuilder.ToString().TrimEnd();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo(suPath)
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var cniHostProcess = new LocalCniHostProcess(process);
        
        await process.StandardInput.WriteLineAsync(new StringBuilder(elevationPassword), cancellationToken);
        await process.StandardInput.WriteLineAsync(new StringBuilder($"{environmentString} {command} ; exit"), cancellationToken);

        return cniHostProcess;
    }
}