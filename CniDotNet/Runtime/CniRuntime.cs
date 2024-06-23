using System.Text.Json;
using CniDotNet.Data;
using CniDotNet.Data.Results;
using CniDotNet.Host;

namespace CniDotNet.Runtime;

public static class CniRuntime
{
    public static async Task<WrappedCniResult<AddCniResult>> AddConfigurationAsync(
        NetworkConfiguration networkConfiguration,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        AddCniResult? previousResult = null;

        foreach (var networkPlugin in networkConfiguration.Plugins)
        {
            var wrappedResult = await AddPluginAsync(
                networkPlugin, runtimeOptions, pluginLookupOptions, previousResult, cancellationToken);
            previousResult = wrappedResult.SuccessValue;
            
            if (wrappedResult.IsError)
            {
                return WrappedCniResult<AddCniResult>.Error(wrappedResult.ErrorValue!);
            }
        }
        
        return WrappedCniResult<AddCniResult>.Success(previousResult!);
    }

    public static async Task<ErrorCniResult?> DeleteConfigurationAsync(
        NetworkConfiguration networkConfiguration,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        for (var i = networkConfiguration.Plugins.Count - 1; i >= 0; i--)
        {
            var networkPlugin = networkConfiguration.Plugins[i];
            var errorCniResult = await DeletePluginAsync(
                networkPlugin, runtimeOptions, pluginLookupOptions, previousResult, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> CheckConfigurationAsync(
        NetworkConfiguration networkConfiguration,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (networkConfiguration.DisableCheck) return null;
        
        foreach (var networkPlugin in networkConfiguration.Plugins)
        {
            var errorCniResult = await CheckPluginAsync(networkPlugin, runtimeOptions, previousResult,
                pluginLookupOptions, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> VerifyConfigurationReadinessAsync(
        NetworkConfiguration networkConfiguration,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var networkPlugin in networkConfiguration.Plugins)
        {
            var errorCniResult = await VerifyPluginReadinessAsync(networkPlugin, runtimeOptions, pluginLookupOptions,
                cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> GarbageCollectConfigurationAsync(
        NetworkConfiguration networkConfiguration,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (networkConfiguration.DisableGc) return null;

        foreach (var networkPlugin in networkConfiguration.Plugins)
        {
            var errorCniResult = await GarbageCollectPluginAsync(networkPlugin, runtimeOptions, pluginLookupOptions,
                cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<WrappedCniResult<AddCniResult>> AddPluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        AddCniResult? previousResult = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(networkPlugin, runtimeOptions, operation: Constants.Operations.Add,
            pluginBinary!, previousResult, cancellationToken);
        return WrapCniResultWithOutput<AddCniResult>(resultJson);
    }
    
    public static async Task<ErrorCniResult?> DeletePluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        AddCniResult? previousResult = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(networkPlugin, runtimeOptions, operation: Constants.Operations.Delete,
            pluginBinary!, previousResult, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    public static async Task<ErrorCniResult?> CheckPluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(networkPlugin, runtimeOptions, operation: Constants.Operations.Check,
            pluginBinary!, previousResult, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    public static async Task<ErrorCniResult?> VerifyPluginReadinessAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(networkPlugin, runtimeOptions, operation: Constants.Operations.Status,
            pluginBinary!, previousResult: null, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }
    
    public static async Task<WrappedCniResult<VersionCniResult>> ProbePluginVersionsAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(networkPlugin, runtimeOptions, operation: Constants.Operations.ProbeVersions,
            pluginBinary!, previousResult: null, cancellationToken);
        return WrapCniResultWithOutput<VersionCniResult>(resultJson);
    }

    public static async Task<ErrorCniResult?> GarbageCollectPluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(networkPlugin, runtimeOptions, operation: Constants.Operations.GarbageCollect,
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
        NetworkPlugin networkPlugin, PluginLookupOptions? pluginLookupOptions)
    {
        pluginLookupOptions ??= PluginLookupOptions.Default;
        var directory = pluginLookupOptions.Directory ??
                        Environment.GetEnvironmentVariable(pluginLookupOptions.EnvironmentVariable);
        if (directory is null) return null;

        if (!cniHost.DirectoryExists(directory)) return null;

        var matchingFiles = cniHost.EnumerateDirectory(
            directory, networkPlugin.Type, pluginLookupOptions.DirectorySearchOption);
        return matchingFiles.FirstOrDefault();
    }
}