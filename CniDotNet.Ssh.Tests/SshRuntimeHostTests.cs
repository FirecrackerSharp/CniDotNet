using AutoFixture.Xunit2;
using CniDotNet.Data.Options;
using FluentAssertions;

namespace CniDotNet.Ssh.Tests;

public class SshRuntimeHostTests : SshFixture
{
    [Theory, AutoData]
    public async Task WriteFileAsync_ShouldPersist(Guid guid, string content)
    {
        var path = $"/tmp/{guid}";
        await Host.WriteFileAsync(path, content, CancellationToken.None);

        var actualContent = Sftp.ReadAllText(path);
        actualContent.Should().Be(content);
    }

    [Theory, AutoData]
    public void DirectoryExists_ShouldReportFalse(Guid guid)
    {
        Host.DirectoryExists($"/tmp/{guid}").Should().BeFalse();
    }

    [Theory, AutoData]
    public void DirectoryExists_ShouldReportTrue(Guid guid)
    {
        var path = $"/tmp/{guid}";
        Sftp.CreateDirectory(path);
        Host.DirectoryExists(path).Should().BeTrue();
    }

    [Theory, AutoData]
    public async Task ReadFileAsync_ShouldReturnCorrectContent(Guid guid, string content)
    {
        var path = $"/tmp/{guid}";
        Sftp.WriteAllText(path, content);
        var actualContent = await Host.ReadFileAsync(path, CancellationToken.None);
        actualContent.Should().Be(content);
    }

    [Theory, AutoData]
    public async Task DeleteFileAsync_ShouldPersist(Guid guid)
    {
        var path = $"/tmp/{guid}";
        Sftp.CreateText(path).Close();
        await Host.DeleteFileAsync(path, CancellationToken.None);
        Sftp.Exists(path).Should().BeFalse();
    }

    [Fact]
    public async Task GetEnvironmentVariableAsync_ShouldSucceed()
    {
        var actualValue = await Host.GetEnvironmentVariableAsync("USER", CancellationToken.None);
        actualValue.Should().NotBeNull();
        actualValue!.TrimEnd('\n').Should().Be("root");
    }

    [Fact]
    public async Task StartProcessAsync_ShouldWorkForRoot()
    {
        var process = await Host.StartProcessAsync("echo test", new Dictionary<string, string>(),
            new InvocationOptions(Host), CancellationToken.None);
        await process.WaitForExitAsync(CancellationToken.None);
        process.CurrentOutput.TrimEnd().Should().Be("test");
    }
}