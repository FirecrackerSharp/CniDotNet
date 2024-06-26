using System.Text;
using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Host;
using Renci.SshNet;

namespace CniDotNet.Host.Ssh;

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
    
    public string GetTempFilePath()
    {
        return $"/tmp/{Guid.NewGuid()}";
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

    public async Task<IRuntimeHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment,
        InvocationOptions invocationOptions, CancellationToken cancellationToken)
    {
        var environmentString = IRuntimeHost.BuildEnvironmentString(environment);
        
        if (connectionInfo.Username == "root")
        {
            var nonElevatedCommand = Ssh.CreateCommand($"{environmentString} {command}");
            return new SshRuntimeHostProcess(nonElevatedCommand, nonElevatedCommand.BeginExecute());
        }

        if (invocationOptions.ElevationPassword is null)
        {
            throw new ElevationFailureException(
                "Need to elevate on SSH host but elevation password hasn't been provided");
        }

        var elevationCommand = Ssh.CreateCommand("su");
        var asyncResult = elevationCommand.BeginExecute();
        var inputStreamWriter = new StreamWriter(elevationCommand.CreateInputStream());
        await inputStreamWriter.WriteLineAsync(new StringBuilder(invocationOptions.ElevationPassword), cancellationToken);
        await inputStreamWriter.WriteLineAsync(new StringBuilder($"{environmentString} {command} ; exit"), cancellationToken);
        return new SshRuntimeHostProcess(elevationCommand, asyncResult);
    }

    public void Dispose()
    {
        _backingSsh?.Disconnect();
        _backingSftp?.Disconnect();
    }
}