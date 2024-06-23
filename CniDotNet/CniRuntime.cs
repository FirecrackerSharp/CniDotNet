using System.Text.Json;
using System.Text.Json.Serialization;
using CniDotNet.Data;
using CniDotNet.Data.Results;
using CniDotNet.Host;

namespace CniDotNet;

public static class CniRuntime
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
    
    public static async Task<WrappedCniResult<AddCniResult>> AddPluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        AddCniResult? previousResult = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(
            networkPlugin,
            runtimeOptions,
            operation: Constants.Operations.Add,
            pluginBinary!,
            previousResult,
            cancellationToken);
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
        var resultJson = await CniInvoker.InvokeAsync(
            networkPlugin,
            runtimeOptions,
            operation: Constants.Operations.Delete,
            pluginBinary!,
            previousResult,
            cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    public static async Task<WrappedCniResult<VersionCniResult>> ProbePluginVersionsAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var resultJson = await CniInvoker.InvokeAsync(
            networkPlugin,
            runtimeOptions,
            operation: Constants.Operations.ProbeVersions,
            pluginBinary!,
            previousResult: null,
            cancellationToken);
        return WrapCniResultWithOutput<VersionCniResult>(resultJson);
    }

    private static WrappedCniResult<T> WrapCniResultWithOutput<T>(string resultJson) where T : class
    {
        if (resultJson.Contains("\"code\": "))
        {
            var errorValue = JsonSerializer.Deserialize<ErrorCniResult>(resultJson);
            return WrappedCniResult<T>.Error(errorValue!);
        }

        var successValue = JsonSerializer.Deserialize<T>(resultJson, SerializerOptions);
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