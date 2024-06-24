using System.Text.Json;
using CniDotNet.Data;
using CniDotNet.Data.Results;
using CniDotNet.Host;

namespace CniDotNet.Runtime;

public static class CniRuntime
{
    public static async Task<WrappedCniResult<AddCniResult>> AddNetworkListAsync(
        NetworkList networkList,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        AddCniResult? previousResult = null;

        foreach (var networkPlugin in networkList.Plugins)
        {
            var wrappedResult = await AddNetworkAsync(
                networkPlugin, cniInvocationOptions, pluginLookupOptions, previousResult, cancellationToken);
            previousResult = wrappedResult.SuccessValue;
            
            if (wrappedResult.IsError)
            {
                return WrappedCniResult<AddCniResult>.Error(wrappedResult.ErrorValue!);
            }
        }
        
        return WrappedCniResult<AddCniResult>.Success(previousResult!);
    }

    public static async Task<ErrorCniResult?> DeleteNetworkListAsync(
        NetworkList networkList,
        CniInvocationOptions cniInvocationOptions,
        AddCniResult previousResult,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        for (var i = networkList.Plugins.Count - 1; i >= 0; i--)
        {
            var networkPlugin = networkList.Plugins[i];
            var errorCniResult = await DeleteNetworkAsync(
                networkPlugin, cniInvocationOptions, pluginLookupOptions, previousResult, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> CheckNetworkListAsync(
        NetworkList networkList,
        CniInvocationOptions cniInvocationOptions,
        AddCniResult previousResult,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (networkList.DisableCheck) return null;
        
        foreach (var networkPlugin in networkList.Plugins)
        {
            var errorCniResult = await CheckNetworkAsync(networkPlugin, cniInvocationOptions, previousResult,
                pluginLookupOptions, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> VerifyNetworkListReadinessAsync(
        NetworkList networkList,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var networkPlugin in networkList.Plugins)
        {
            var errorCniResult = await VerifyNetworkReadinessAsync(networkPlugin, cniInvocationOptions, pluginLookupOptions,
                cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> GarbageCollectNetworkListAsync(
        NetworkList networkList,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (networkList.DisableGc) return null;

        foreach (var networkPlugin in networkList.Plugins)
        {
            var errorCniResult = await GarbageCollectNetworkAsync(networkPlugin, cniInvocationOptions, pluginLookupOptions,
                cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<WrappedCniResult<AddCniResult>> AddNetworkAsync(
        Network network,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        AddCniResult? previousResult = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(cniInvocationOptions.InvocationOptions.CniHost, network, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(network, cniInvocationOptions, operation: Constants.Operations.Add,
            pluginBinary!, previousResult, cancellationToken);
        return WrapCniResultWithOutput<AddCniResult>(resultJson);
    }
    
    public static async Task<ErrorCniResult?> DeleteNetworkAsync(
        Network network,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        AddCniResult? previousResult = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(cniInvocationOptions.InvocationOptions.CniHost, network, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(network, cniInvocationOptions, operation: Constants.Operations.Delete,
            pluginBinary!, previousResult, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    public static async Task<ErrorCniResult?> CheckNetworkAsync(
        Network network,
        CniInvocationOptions cniInvocationOptions,
        AddCniResult previousResult,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(cniInvocationOptions.InvocationOptions.CniHost, network, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(network, cniInvocationOptions, operation: Constants.Operations.Check,
            pluginBinary!, previousResult, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    public static async Task<ErrorCniResult?> VerifyNetworkReadinessAsync(
        Network network,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(cniInvocationOptions.InvocationOptions.CniHost, network, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(network, cniInvocationOptions, operation: Constants.Operations.Status,
            pluginBinary!, previousResult: null, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }
    
    public static async Task<WrappedCniResult<VersionCniResult>> ProbeNetworkVersionsAsync(
        Network network,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(cniInvocationOptions.InvocationOptions.CniHost, network, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(network, cniInvocationOptions, operation: Constants.Operations.ProbeVersions,
            pluginBinary!, previousResult: null, cancellationToken);
        return WrapCniResultWithOutput<VersionCniResult>(resultJson);
    }

    public static async Task<ErrorCniResult?> GarbageCollectNetworkAsync(
        Network network,
        CniInvocationOptions cniInvocationOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(cniInvocationOptions.InvocationOptions.CniHost, network, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(network, cniInvocationOptions, operation: Constants.Operations.GarbageCollect,
            pluginBinary!, previousResult: null, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    private static WrappedCniResult<T> WrapCniResultWithOutput<T>(string resultJson) where T : class
    {
        if (resultJson.Contains("\"code\": "))
        {
            var errorValue = JsonSerializer.Deserialize<ErrorCniResult>(resultJson);
            return WrappedCniResult<T>.Error(errorValue!);
        }

        var successValue = JsonSerializer.Deserialize<T>(resultJson);
        return WrappedCniResult<T>.Success(successValue!);
    }

    private static ErrorCniResult? WrapCniResultWithoutOutput(string resultJson)
    {
        return string.IsNullOrWhiteSpace(resultJson)
            ? null
            : JsonSerializer.Deserialize<ErrorCniResult>(resultJson);
    }

    private static string? LookupPluginBinary(ICniHost cniHost,
        Network network, PluginLookupOptions? pluginLookupOptions)
    {
        pluginLookupOptions ??= PluginLookupOptions.Default;
        var directory = pluginLookupOptions.Directory ??
                        Environment.GetEnvironmentVariable(pluginLookupOptions.EnvironmentVariable);
        if (directory is null) return null;

        if (!cniHost.DirectoryExists(directory)) return null;

        var matchingFiles = cniHost.EnumerateDirectory(
            directory, network.Type, pluginLookupOptions.DirectorySearchOption);
        return matchingFiles.FirstOrDefault();
    }
}