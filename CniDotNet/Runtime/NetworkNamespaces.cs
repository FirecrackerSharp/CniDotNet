using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data;

namespace CniDotNet.Runtime;

public static class NetworkNamespaces
{
    public static async Task<IReadOnlyList<NetworkNamespace>> GetAllAsync(
        InvocationOptions invocationOptions,
        CancellationToken cancellationToken = default)
    {
        var json = await RunAndGetAsync($"ip -j netns list", invocationOptions, cancellationToken);
        if (json is null) return [];
        
        JsonArray jsonArray;
        try
        {
            jsonArray = JsonSerializer.Deserialize<JsonArray>(json)!;
        }
        catch (Exception)
        {
            return [];
        }

        var namespaces = new List<NetworkNamespace>();
        
        foreach (var jsonNode in jsonArray)
        {
            var name = jsonNode!["name"]!.GetValue<string>();
            uint? id = null;

            if (jsonNode.AsObject().ContainsKey("id"))
            {
                id = jsonNode["id"]!.GetValue<uint>();
            }
            
            namespaces.Add(new NetworkNamespace(name, id));
        }

        return namespaces;
    }

    public static async Task<string?> AddAsync(
        NetworkNamespace networkNamespace,
        InvocationOptions invocationOptions,
        CancellationToken cancellationToken = default)
    {
        var output =
            await RunAndGetAsync($"ip netns add {networkNamespace.Name}", invocationOptions, cancellationToken);
        if (output is not null) return output;

        if (networkNamespace.Id is not null)
        {
            return await AssignNamespaceIdAsync(networkNamespace.Name, networkNamespace.Id.Value, invocationOptions,
                cancellationToken);
        }

        return null;
    }

    public static Task<string?> AssignNamespaceIdAsync(
        string namespaceName,
        uint namespaceId,
        InvocationOptions invocationOptions,
        CancellationToken cancellationToken = default) =>
        RunAndGetAsync($"ip netns set {namespaceName} {namespaceId}", invocationOptions, cancellationToken);

    public static Task<string?> DeleteAsync(
        string namespaceName,
        InvocationOptions invocationOptions,
        CancellationToken cancellationToken = default) =>
        RunAndGetAsync($"ip netns del {namespaceName}", invocationOptions, cancellationToken);

    private static async Task<string?> RunAndGetAsync(
        string command,
        InvocationOptions invocationOptions,
        CancellationToken cancellationToken = default)
    {
        var process = await invocationOptions.CniHost.StartProcessAsync(
            command, new Dictionary<string, string>(),
            invocationOptions.ElevationPassword!, invocationOptions.SuPath, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return string.IsNullOrEmpty(process.CurrentOutput) ? null : process.CurrentOutput;
    }
}