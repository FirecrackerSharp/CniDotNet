using System.Diagnostics;
using System.Text;
using CniDotNet.Data;

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

    public Task<ICniHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        if (Environment.UserName == "root")
        {
            return StartProcessWithoutElevationAsync(command, environment, invocationOptions, cancellationToken);
        }

        if (invocationOptions.ElevationPassword is null)
        {
            throw new ElevationFailureException(
                "Need to elevate on local host but elevation process is not provided");
        }

        return StartProcessWithElevationAsync(command, environment, invocationOptions, cancellationToken);
    }

    private static async Task<ICniHostProcess> StartProcessWithoutElevationAsync(
        string command, Dictionary<string, string> environment, InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        var environmentString = BuildEnvironmentString(environment);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo(invocationOptions.BashPath)
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var cniHostProcess = new LocalCniHostProcess(process);

        await process.StandardInput.WriteLineAsync(new StringBuilder($"{environmentString} {command} ; exit"), cancellationToken);

        return cniHostProcess;
    }
    
    private static async Task<ICniHostProcess> StartProcessWithElevationAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        var environmentString = BuildEnvironmentString(environment);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo(invocationOptions.SuPath)
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var cniHostProcess = new LocalCniHostProcess(process);
        
        await process.StandardInput.WriteLineAsync(new StringBuilder(invocationOptions.ElevationPassword), cancellationToken);
        await process.StandardInput.WriteLineAsync(new StringBuilder($"{environmentString} {command} ; exit"), cancellationToken);

        return cniHostProcess;
    }

    private static string BuildEnvironmentString(Dictionary<string, string> environment)
    {
        var environmentBuilder = new StringBuilder();

        foreach (var (key, value) in environment)
        {
            environmentBuilder.Append($"{key}={value} ");
        }

        return environmentBuilder.ToString().TrimEnd();
    }
}