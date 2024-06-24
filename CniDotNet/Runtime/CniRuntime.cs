using System.Text.Json;
using System.Text.Json.Serialization;
using CniDotNet.Data;
using CniDotNet.Data.Results;

namespace CniDotNet.Runtime;

public static class CniRuntime
{
    public static CniLock MutativeOperationLock { get; } = new();
    
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal static readonly JsonSerializerOptions PrettyPrintSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
    
    public static async Task<WrappedCniResult<AddCniResult>> AddPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        AddCniResult? previousResult = null;

        foreach (var plugin in pluginList.Plugins)
        {
            var wrappedResult = await AddPluginAsync(
                plugin, runtimeOptions, previousResult, cancellationToken);
            previousResult = wrappedResult.SuccessValue;
            
            if (wrappedResult.IsError)
            {
                return WrappedCniResult<AddCniResult>.Error(wrappedResult.ErrorValue!);
            }
        }
        
        return WrappedCniResult<AddCniResult>.Success(previousResult!);
    }

    public static async Task<ErrorCniResult?> DeletePluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        CancellationToken cancellationToken = default)
    {
        for (var i = pluginList.Plugins.Count - 1; i >= 0; i--)
        {
            var plugin = pluginList.Plugins[i];
            var errorCniResult = await DeletePluginAsync(
                plugin, runtimeOptions, previousResult, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> CheckPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        CancellationToken cancellationToken = default)
    {
        if (pluginList.DisableCheck) return null;
        
        foreach (var plugin in pluginList.Plugins)
        {
            var errorCniResult = await CheckPluginAsync(plugin, runtimeOptions, previousResult, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> VerifyPluginListReadinessAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        foreach (var plugin in pluginList.Plugins)
        {
            var errorCniResult = await VerifyPluginReadinessAsync(plugin, runtimeOptions, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<WrappedCniResult<IReadOnlyDictionary<Plugin, VersionCniResult>>> ProbePluginListVersionsAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<Plugin, VersionCniResult>();

        foreach (var plugin in pluginList.Plugins)
        {
            var wrappedResult = await ProbePluginVersionsAsync(plugin, runtimeOptions, cancellationToken);

            if (wrappedResult.IsSuccess)
            {
                results[plugin] = wrappedResult.SuccessValue!;
            }
            else
            {
                return WrappedCniResult<IReadOnlyDictionary<Plugin, VersionCniResult>>.Error(wrappedResult.ErrorValue!);
            }
        }
        
        return WrappedCniResult<IReadOnlyDictionary<Plugin, VersionCniResult>>.Success(results);
    }

    public static async Task<ErrorCniResult?> GarbageCollectPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (pluginList.DisableGc) return null;

        foreach (var plugin in pluginList.Plugins)
        {
            var errorCniResult = await GarbageCollectPluginAsync(plugin, runtimeOptions, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<WrappedCniResult<AddCniResult>> AddPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult? previousResult = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await MutativeOperationLock.Semaphore.WaitAsync(cancellationToken);
            var pluginBinary = SearchForPluginBinary(plugin, runtimeOptions);
            var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Add,
                pluginBinary!, previousResult, cancellationToken);
            return WrapCniResultWithOutput<AddCniResult>(resultJson);
        }
        finally
        {
            MutativeOperationLock.Semaphore.Release();
        }
    }
    
    public static async Task<ErrorCniResult?> DeletePluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult? previousResult = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await MutativeOperationLock.Semaphore.WaitAsync(cancellationToken);
            var pluginBinary = SearchForPluginBinary(plugin, runtimeOptions);
            var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Delete,
                pluginBinary!, previousResult, cancellationToken);
            return WrapCniResultWithoutOutput(resultJson);
        }
        finally
        {
            MutativeOperationLock.Semaphore.Release();
        }
    }

    public static async Task<ErrorCniResult?> CheckPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = SearchForPluginBinary(plugin, runtimeOptions);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Check,
            pluginBinary!, previousResult, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    public static async Task<ErrorCniResult?> VerifyPluginReadinessAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = SearchForPluginBinary(plugin, runtimeOptions);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Status,
            pluginBinary!, previousResult: null, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }
    
    public static async Task<WrappedCniResult<VersionCniResult>> ProbePluginVersionsAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = SearchForPluginBinary(plugin, runtimeOptions);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.ProbeVersions,
            pluginBinary!, previousResult: null, cancellationToken);
        return WrapCniResultWithOutput<VersionCniResult>(resultJson);
    }

    public static async Task<ErrorCniResult?> GarbageCollectPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await MutativeOperationLock.Semaphore.WaitAsync(cancellationToken);
            var pluginBinary = SearchForPluginBinary(plugin, runtimeOptions);
            var resultJson = await InvokeAsync(plugin, runtimeOptions,
                operation: Constants.Operations.GarbageCollect,
                pluginBinary!, previousResult: null, cancellationToken);
            return WrapCniResultWithoutOutput(resultJson);
        }
        finally
        {
            MutativeOperationLock.Semaphore.Release();
        }
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

    private static string? SearchForPluginBinary(Plugin plugin, RuntimeOptions runtimeOptions)
    {
        var directory = runtimeOptions.PluginSearchOptions.ActualDirectory;
        if (directory is null) return null;

        if (!runtimeOptions.InvocationOptions.CniHost.DirectoryExists(directory)) return null;

        var matchingFiles = runtimeOptions.InvocationOptions.CniHost.EnumerateDirectory(
            directory, plugin.Type, runtimeOptions.PluginSearchOptions.DirectorySearchOption);
        return matchingFiles.FirstOrDefault();
    }

    private static async Task<string> InvokeAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        AddCniResult? previousResult,
        CancellationToken cancellationToken)
    {
        var stdinJson = DerivePluginInput(plugin, runtimeOptions.CniVersion!,
            runtimeOptions.ContainerId, previousResult);
        var inputPath = runtimeOptions.InvocationOptions.CniHost.GetTempFilePath();
        await runtimeOptions.InvocationOptions.CniHost.WriteFileAsync(inputPath, stdinJson, cancellationToken);
        
        var environment = new Dictionary<string, string>
        {
            { Constants.Environment.Command, operation },
            { Constants.Environment.ContainerId, runtimeOptions.ContainerId },
            { Constants.Environment.InterfaceName, runtimeOptions.InterfaceName },
            { Constants.Environment.NetworkNamespace, runtimeOptions.NetworkNamespace }
        };
        if (runtimeOptions.PluginSearchOptions.ActualDirectory is not null)
        {
            environment[Constants.Environment.PluginPath] = runtimeOptions.PluginSearchOptions.ActualDirectory;
        }

        var process = await runtimeOptions.InvocationOptions.CniHost.StartProcessAsync(
            $"{pluginBinary} < {inputPath}", environment, runtimeOptions.InvocationOptions, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        runtimeOptions.InvocationOptions.CniHost.DeleteFile(inputPath);
        
        return process.CurrentOutput;
    }
    
    private static string DerivePluginInput(Plugin plugin, string cniVersion, string name,
        AddCniResult? previousResult)
    {
        var jsonNode = plugin.PluginParameters.DeepClone();
        jsonNode[Constants.Parsing.CniVersion] = cniVersion;
        jsonNode[Constants.Parsing.Name] = name;
        jsonNode[Constants.Parsing.Type] = plugin.Type;

        if (plugin.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] = plugin.Capabilities;
        }

        if (previousResult is not null)
        {
            jsonNode[Constants.Parsing.PreviousResult] = JsonSerializer
                .SerializeToNode(previousResult, SerializerOptions)!.AsObject();
        }

        return JsonSerializer.Serialize(jsonNode, SerializerOptions);
    }
}