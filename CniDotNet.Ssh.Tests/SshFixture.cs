using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Renci.SshNet;

namespace CniDotNet.Ssh.Tests;

public class SshFixture : IAsyncLifetime
{
    private static ConnectionInfo? _connectionInfo;
    private static ConnectionInfo? _rootlessConnectionInfo;
    private static IContainer? _container;

    private SshClient Ssh { get; set; } = null!;
    protected SftpClient Sftp { get; private set; } = null!;
    protected SshRuntimeHost Host { get; private set; } = null!;
    protected SshRuntimeHost RootlessHost { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        if (_container is not null)
        {
            await ConnectAsync();
            return;
        }

        var hostSshPort = Random.Shared.Next(10000, 65536);
        _container = new ContainerBuilder()
            .WithImage("ssh_server:latest")
            .WithPortBinding(hostSshPort, containerPort: 22)
            .Build();
        await _container.StartAsync();
        await Task.Delay(100); // wait for init

        _connectionInfo = new ConnectionInfo(
            "127.0.0.1", hostSshPort, "root", new PasswordAuthenticationMethod("root", "root123"));
        _rootlessConnectionInfo = new ConnectionInfo(
            "127.0.0.1", hostSshPort, "regular", new PasswordAuthenticationMethod("regular", "regular"));
        await ConnectAsync();
    }

    public Task DisposeAsync()
    {
        Ssh.Disconnect();
        Sftp.Disconnect();
        return Task.CompletedTask;
    }

    private async Task ConnectAsync()
    {
        Ssh = new SshClient(_connectionInfo);
        await Ssh.ConnectAsync(CancellationToken.None);

        Sftp = new SftpClient(_connectionInfo);
        await Sftp.ConnectAsync(CancellationToken.None);

        Host = new SshRuntimeHost(_connectionInfo ?? throw new ArgumentNullException());
        RootlessHost = new SshRuntimeHost(_rootlessConnectionInfo ?? throw new ArgumentNullException());
    }
}