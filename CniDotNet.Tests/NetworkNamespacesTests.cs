using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoFixture.Xunit2;
using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using FluentAssertions;

namespace CniDotNet.Tests;

public class NetworkNamespacesTests
{
    private readonly InvocationOptions _invocationOptions =
        new(LocalRuntimeHost.Instance, Environment.GetEnvironmentVariable("ROOT_PWD"));
    
    [Theory, AutoData]
    public async Task GetAllAsync_ShouldRetrieve(string name)
    {
        await ExecAsync($"ip netns add {name}");
        
        var actual = await NetworkNamespaces.GetAllAsync(_invocationOptions);
        var expected = await GetNetNsAsync();
        actual.Should().BeEquivalentTo(expected);

        await ExecAsync($"ip netns del {name}");
    }

    [Theory, AutoData]
    public async Task AddAsync_ShouldAddWithId(NetworkNamespace networkNamespace)
    {
        var error = await NetworkNamespaces.AddAsync(networkNamespace, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().Contain(networkNamespace);
        
        await ExecAsync($"ip netns del {networkNamespace.Name}");
    }

    [Theory, AutoData]
    public async Task AddAsync_ShouldAddWithoutId(NetworkNamespace networkNamespace)
    {
        networkNamespace = networkNamespace with { Id = null };
        
        var error = await NetworkNamespaces.AddAsync(networkNamespace, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().Contain(networkNamespace);

        await ExecAsync($"ip netns del {networkNamespace.Name}");
    }

    [Theory, AutoData]
    public async Task AssignNamespaceIdAsync_ShouldPerformAssignment(string namespaceName, uint namespaceId)
    {
        await ExecAsync($"ip netns add {namespaceName}");
        
        var error = await NetworkNamespaces.AssignNamespaceIdAsync(namespaceName, namespaceId, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().Contain(n => n.Name == namespaceName && n.Id == namespaceId);

        await ExecAsync($"ip netns del {namespaceName}");
    }

    [Theory, AutoData]
    public async Task DeleteAsync_ShouldRemove(string namespaceName)
    {
        await ExecAsync($"ip netns add {namespaceName}");

        var error = await NetworkNamespaces.DeleteAsync(namespaceName, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().NotContain(n => n.Name == namespaceName);
    }

    private static async Task<IReadOnlyList<NetworkNamespace>> GetNetNsAsync()
    {
        var json = await ExecAsync("ip -j netns list");
        return JsonSerializer
            .Deserialize<IEnumerable<TransferNetNs>>(json)!
            .Select(n => new NetworkNamespace(n.Name, n.Id))
            .ToList();
    }
    
    private static async Task<string> ExecAsync(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("/bin/su")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            }
        };

        process.Start();
        
        var buffer = new StringBuilder();
        process.BeginOutputReadLine();

        await process.StandardInput.WriteLineAsync(Environment.GetEnvironmentVariable("ROOT_PWD"));
        await process.StandardInput.WriteLineAsync($"{command} ; exit");
        
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                buffer.Append(args.Data);
            }
        };
        
        await process.WaitForExitAsync();
        return buffer.ToString();
    }

    private record TransferNetNs(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("id")] uint? Id = null);
}