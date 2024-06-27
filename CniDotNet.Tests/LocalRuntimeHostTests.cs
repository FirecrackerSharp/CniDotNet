using AutoFixture.Xunit2;
using CniDotNet.Abstractions;
using FluentAssertions;

namespace CniDotNet.Tests;

public class LocalRuntimeHostTests
{
    [Theory, AutoData]
    public async Task WriteFileAsync_ShouldPersist(string content)
    {
        var path = $"/tmp/{Guid.NewGuid()}";
        await LocalRuntimeHost.Instance.WriteFileAsync(path, content, CancellationToken.None);
        var actualContent = await File.ReadAllTextAsync(path);
        actualContent.Should().Be(content);
        File.Delete(path);
    }

    [Fact]
    public void DirectoryExists_ShouldReturnFalse_ForNonExistentDirectory()
    {
        var path = $"/tmp/{Guid.NewGuid()}";
        LocalRuntimeHost.Instance.DirectoryExists(path).Should().BeFalse();
    }

    [Fact]
    public void DirectoryExists_ShouldReturnTrue_ForExistentDirectory()
    {
        var path = $"/tmp/{Guid.NewGuid()}";
        Directory.CreateDirectory(path);
        LocalRuntimeHost.Instance.DirectoryExists(path).Should().BeTrue();
        Directory.Delete(path);
    }

    [Theory, AutoData]
    public async Task ReadFileAsync_ShouldReturnContent(string content)
    {
        var path = $"/tmp/{Guid.NewGuid()}";
        await File.WriteAllTextAsync(path, content);
        var actualContent = await LocalRuntimeHost.Instance.ReadFileAsync(path, CancellationToken.None);
        actualContent.Should().Be(content);
        File.Delete(path);
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldDelete()
    {
        var path = $"/tmp/{Guid.NewGuid()}";
        File.CreateText(path).Close();
        await LocalRuntimeHost.Instance.DeleteFileAsync(path, CancellationToken.None);
        File.Exists(path).Should().BeFalse();
    }

    [Theory, AutoData]
    public async Task GetEnvironmentVariableAsync_ShouldReturnOne(string variableName, string variableValue)
    {
        Environment.SetEnvironmentVariable(variableName, variableValue, EnvironmentVariableTarget.Process);
        var actualVariableValue =
            await LocalRuntimeHost.Instance.GetEnvironmentVariableAsync(variableName, CancellationToken.None);
        actualVariableValue.Should().Be(variableValue);
        Environment.SetEnvironmentVariable(variableName, "", EnvironmentVariableTarget.Process);
    }
}