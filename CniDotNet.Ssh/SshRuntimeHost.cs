using System.Text;
using CniDotNet.Abstractions;
using CniDotNet.Data.Options;
using Renci.SshNet;

namespace CniDotNet.Ssh;

public sealed class SshRuntimeHost(ConnectionInfo connectionInfo) : IRuntimeHost, IDisposable
{
    private SshClient? _backingSsh;
    private SftpClient? _backingSftp;

    private SshClient Ssh
    {
        get
        {
            if (_backingSsh is not null && _backingSsh.IsConnected) return _backingSsh;
            
            _backingSsh = new SshClient(connectionInfo);
            _backingSsh.Connect();
            return _backingSsh;
        }
    }

    private SftpClient Sftp
    {
        get
        {
            if (_backingSftp is not null && _backingSftp.IsConnected) return _backingSftp;

            _backingSftp = new SftpClient(connectionInfo);
            _backingSftp.Connect();
            return _backingSftp;
        }
    }

    public async Task WriteFileAsync(string path, string content, CancellationToken cancellationToken)
    {
        await using var fileStream = await Sftp.OpenAsync(path, FileMode.OpenOrCreate, FileAccess.Write, cancellationToken);
        await fileStream.WriteAsync(new ReadOnlyMemory<byte>(Encoding.Default.GetBytes(content)), cancellationToken);
    }

    public bool DirectoryExists(string path)
    {
        return Sftp.Exists(path);
    }

    public async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        await using var fileStream = await Sftp.OpenAsync(path, FileMode.OpenOrCreate, FileAccess.Read, cancellationToken);
        using var streamReader = new StreamReader(fileStream);
        return await streamReader.ReadToEndAsync(cancellationToken);
    }

    public async Task DeleteFileAsync(string path, CancellationToken cancellationToken)
    {
        await Sftp.DeleteFileAsync(path, cancellationToken);
    }

    public async Task<IEnumerable<string>> EnumerateDirectoryAsync(string path, string searchPattern,
        SearchOption searchOption, CancellationToken cancellationToken)
    {
        var outputs = new List<string>();

        await foreach (var sftpFile in Sftp.ListDirectoryAsync(path, cancellationToken))
        {
            if (sftpFile.Name.Contains(searchPattern))
            {
                outputs.Add(sftpFile.FullName);
            }
        }

        return outputs;
    }

    public async Task<string?> GetEnvironmentVariableAsync(string variableName, CancellationToken cancellationToken)
    {
        var sshCommand = Ssh.CreateCommand($"echo ${variableName}");
        var asyncResult = sshCommand.BeginExecute();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (asyncResult.IsCompleted) break;
            await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
        }

        return sshCommand.EndExecute(asyncResult);
    }

    public async Task<IRuntimeHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        var environmentString = IRuntimeHost.BuildEnvironmentString(environment);

        if (connectionInfo.Username != "root")
        {
            throw new ElevationFailureException("Elevation is not supported with SSH. Instead, connect as root");
        }

        var nonElevatedCommand = Ssh.CreateCommand($"{environmentString} {command}");
        return new SshRuntimeHostProcess(nonElevatedCommand, nonElevatedCommand.BeginExecute());

    }

    public void Dispose()
    {
        _backingSsh?.Disconnect();
        _backingSftp?.Disconnect();
    }
}