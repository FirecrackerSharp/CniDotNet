using System.Text.Json;
using System.Text.Json.Serialization;
using AutoFixture.Xunit2;
using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class NetworkNamespacesTests
{
    private readonly InvocationOptions _invocationOptions =
        new(LocalRuntimeHost.Instance, Environment.GetEnvironmentVariable("ROOT_PWD"));
    
    [Theory, AutoData]
    public async Task GetAllAsync_ShouldRetrieve(string name)
    {
        await Exec.CommandAsync($"ip netns add {name}");
        
        var actual = await NetworkNamespaces.GetAllAsync(_invocationOptions);
        var expected = await GetNetNsAsync();
        actual.Should().BeEquivalentTo(expected);

        await Exec.CommandAsync($"ip netns del {name}");
    }

    [Theory, AutoData]
    public async Task AddAsync_ShouldAddWithId(NetworkNamespace networkNamespace)
    {
        var error = await NetworkNamespaces.AddAsync(networkNamespace, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().Contain(networkNamespace);
        
        await Exec.CommandAsync($"ip netns del {networkNamespace.Name}");
    }

    [Theory, AutoData]
    public async Task AddAsync_ShouldAddWithoutId(NetworkNamespace networkNamespace)
    {
        networkNamespace = networkNamespace with { Id = null };
        
        var error = await NetworkNamespaces.AddAsync(networkNamespace, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().Contain(networkNamespace);

        await Exec.CommandAsync($"ip netns del {networkNamespace.Name}");
    }

    [Theory, AutoData]
    public async Task AssignNamespaceIdAsync_ShouldPerformAssignment(string namespaceName, uint namespaceId)
    {
        await Exec.CommandAsync($"ip netns add {namespaceName}");
        
        var error = await NetworkNamespaces.AssignNamespaceIdAsync(namespaceName, namespaceId, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().Contain(n => n.Name == namespaceName && n.Id == namespaceId);

        await Exec.CommandAsync($"ip netns del {namespaceName}");
    }

    [Theory, AutoData]
    public async Task DeleteAsync_ShouldRemove(string namespaceName)
    {
        await Exec.CommandAsync($"ip netns add {namespaceName}");

        var error = await NetworkNamespaces.DeleteAsync(namespaceName, _invocationOptions);
        error.Should().BeNull();
        var namespaces = await GetNetNsAsync();
        namespaces.Should().NotContain(n => n.Name == namespaceName);
    }

    private static async Task<IReadOnlyList<NetworkNamespace>> GetNetNsAsync()
    {
        var json = await Exec.CommandAsync("ip -j netns list");
        return JsonSerializer
            .Deserialize<IEnumerable<TransferNetNs>>(json)!
            .Select(n => new NetworkNamespace(n.Name, n.Id))
            .ToList();
    }

    private record TransferNetNs(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("id")] uint? Id = null);
}