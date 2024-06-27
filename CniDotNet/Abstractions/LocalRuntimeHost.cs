using System.Diagnostics;
using System.Text;
using CniDotNet.Data.Options;

namespace CniDotNet.Abstractions;

public sealed class LocalRuntimeHost : IRuntimeHost
{
    public static LocalRuntimeHost Instance { get; } = new();
    
    private LocalRuntimeHost() {}

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

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken)
    {
        File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> EnumerateDirectoryAsync(string path, string searchPattern,
        SearchOption searchOption, CancellationToken cancellationToken)
    {
        return Task.FromResult(Directory.EnumerateFiles(path, searchPattern, searchOption));
    }

    public Task<string?> GetEnvironmentVariableAsync(string variableName, CancellationToken cancellationToken)
    {
        return Task.FromResult(Environment.GetEnvironmentVariable(variableName));
    }

    public Task<IRuntimeHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        if (Environment.UserName == "root")
        {
            return StartProcessWithoutElevationAsync(command, environment, invocationOptions, cancellationToken);
        }

        if (invocationOptions.ElevationPassword is null)
        {
            throw new ElevationFailureException(
                "Need to elevate on local host but elevation process hasn't been provided");
        }

        return StartProcessWithElevationAsync(command, environment, invocationOptions, cancellationToken);
    }

    private static async Task<IRuntimeHostProcess> StartProcessWithoutElevationAsync(
        string command, Dictionary<string, string> environment, InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        var environmentString = IRuntimeHost.BuildEnvironmentString(environment);
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
        var cniHostProcess = new LocalRuntimeHostProcess(process);

        await process.StandardInput.WriteLineAsync(new StringBuilder($"{environmentString} {command} ; exit"), cancellationToken);

        return cniHostProcess;
    }
    
    private static async Task<IRuntimeHostProcess> StartProcessWithElevationAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        var environmentString = IRuntimeHost.BuildEnvironmentString(environment);
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
        var cniHostProcess = new LocalRuntimeHostProcess(process);
        
        await process.StandardInput.WriteLineAsync(new StringBuilder(invocationOptions.ElevationPassword), cancellationToken);
        await process.StandardInput.WriteLineAsync(new StringBuilder($"{environmentString} {command} ; exit"), cancellationToken);

        return cniHostProcess;
    }
}