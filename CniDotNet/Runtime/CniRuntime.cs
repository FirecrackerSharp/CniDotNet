using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CniDotNet.Data;
using CniDotNet.Data.Results;

namespace CniDotNet.Runtime;

public static partial class CniRuntime
{
    public static CniLock MutativeOperationLock { get; } = new();

    public const PluginOptionRequirement AddRequirements = PluginOptionRequirement.ContainerId |
                                                           PluginOptionRequirement.InterfaceName |
                                                           PluginOptionRequirement.NetworkNamespace;
    public const PluginOptionRequirement DeleteRequirements = PluginOptionRequirement.ContainerId |
                                                              PluginOptionRequirement.InterfaceName;
    public const PluginOptionRequirement CheckRequirements = AddRequirements;
    public const PluginOptionRequirement ProbeVersionsRequirements = 0; // none
    public const PluginOptionRequirement VerifyReadinessRequirements = 0; // none
    public const PluginOptionRequirement GarbageCollectRequirements = PluginOptionRequirement.Path;
    
    private static readonly Regex CniRegex = CniRegexGenerator();
    private const int MaximumInterfaceNameLength = 15;
    
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
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);
        try
        {
            await MutativeOperationLock.Semaphore.WaitAsync(cancellationToken);
            var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
            var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Add,
                pluginBinary, previousResult, cancellationToken);
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
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);
        try
        {
            await MutativeOperationLock.Semaphore.WaitAsync(cancellationToken);
            var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
            var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Delete,
                pluginBinary, previousResult, cancellationToken);
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
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Check, CheckRequirements);
        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Check,
            pluginBinary, previousResult, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }

    public static async Task<ErrorCniResult?> VerifyPluginReadinessAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Status, VerifyReadinessRequirements);
        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Status,
            pluginBinary, previousResult: null, cancellationToken);
        return WrapCniResultWithoutOutput(resultJson);
    }
    
    public static async Task<WrappedCniResult<VersionCniResult>> ProbePluginVersionsAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.ProbeVersions, ProbeVersionsRequirements);
        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.ProbeVersions,
            pluginBinary, previousResult: null, cancellationToken);
        return WrapCniResultWithOutput<VersionCniResult>(resultJson);
    }

    public static async Task<ErrorCniResult?> GarbageCollectPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.GarbageCollect, GarbageCollectRequirements);
        try
        {
            await MutativeOperationLock.Semaphore.WaitAsync(cancellationToken);
            var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
            var resultJson = await InvokeAsync(plugin, runtimeOptions,
                operation: Constants.Operations.GarbageCollect,
                pluginBinary, previousResult: null, cancellationToken);
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

    private static void ValidatePluginOptions(PluginOptions pluginOptions, string operation,
        PluginOptionRequirement requirement)
    {
        // path
        if (requirement.HasFlag(PluginOptionRequirement.Path) && !pluginOptions.IncludePath)
        {
            throw new PluginOptionValidationException(
                $"Path is required for \"{operation}\" but is excluded according to IncludePath=false");
        }
        
        // network namespace
        if (requirement.HasFlag(PluginOptionRequirement.NetworkNamespace) && string.IsNullOrWhiteSpace(pluginOptions.NetworkNamespace))
        {
            throw new PluginOptionValidationException(
                $"Network namespace is required for \"{operation}\" but isn't provided");
        }
        
        // network name
        if (string.IsNullOrWhiteSpace(pluginOptions.Name))
        {
            throw new PluginOptionValidationException("Network name is required for any operation but is missing");
        }

        if (!CniRegex.IsMatch(pluginOptions.Name))
        {
            throw new PluginOptionValidationException($"Network name \"{pluginOptions.Name}\" doesn't match regex");
        }
        
        // container ID
        if (requirement.HasFlag(PluginOptionRequirement.ContainerId) && string.IsNullOrWhiteSpace(pluginOptions.ContainerId))
        {
            throw new PluginOptionValidationException($"Container ID is required for \"{operation}\" but isn't provided");
        }
        
        if (pluginOptions.ContainerId is not null && !CniRegex.IsMatch(pluginOptions.ContainerId))
        {
            throw new PluginOptionValidationException($"Container ID \"{pluginOptions.ContainerId}\" doesn't match regex");
        }
        
        // interface name
        if (requirement.HasFlag(PluginOptionRequirement.InterfaceName) && string.IsNullOrWhiteSpace(pluginOptions.InterfaceName))
        {
            throw new PluginOptionValidationException($"Interface name is required for \"{operation}\" but isn't provided");
        }

        if (pluginOptions.InterfaceName is null) return;

        if (pluginOptions.InterfaceName.Length > MaximumInterfaceNameLength)
        {
            throw new PluginOptionValidationException(
                $"Interface name \"{pluginOptions.InterfaceName}\" is longer than the maximum of {MaximumInterfaceNameLength}");
        }

        if (pluginOptions.InterfaceName is "." or "..")
        {
            throw new PluginOptionValidationException("Interface name is either . or .., neither of which are allowed");
        }

        if (pluginOptions.InterfaceName.Any(c => c is '/' or ':' || char.IsWhiteSpace(c)))
        {
            throw new PluginOptionValidationException(
                $"Interface name \"{pluginOptions.InterfaceName}\" contains a forbidden character (/, : or a space)");
        }
    }

    private static async Task<string> SearchForPluginBinaryAsync(Plugin plugin, RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken)
    {
        var matchFromTable = runtimeOptions.PluginSearchOptions.SearchTable?.GetValueOrDefault(plugin.Type);
        if (matchFromTable is not null) return matchFromTable;

        var directory = runtimeOptions.PluginSearchOptions.ActualDirectory;
        if (directory is null)
        {
            throw new PluginBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: directory wasn't specified and " +
                                              $"environment variable doesn't exist");
        }

        if (!runtimeOptions.InvocationOptions.CniHost.DirectoryExists(directory))
        {
            throw new PluginBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: \"{directory}\" directory " +
                                              $"doesn't exist");
        }

        var matchingFiles = await runtimeOptions.InvocationOptions.CniHost.EnumerateDirectoryAsync(
            directory, plugin.Type, runtimeOptions.PluginSearchOptions.DirectorySearchOption,
            cancellationToken);
        return matchingFiles.FirstOrDefault() ?? throw new PluginBinaryNotFoundException($"Could not find \"{plugin.Type}\" " +
            $"plugin: the file doesn't exist according to the given search option in the \"{directory}\" directory");
    }

    private static async Task<string> InvokeAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        AddCniResult? previousResult,
        CancellationToken cancellationToken)
    {
        var stdinJson = DerivePluginInput(plugin, runtimeOptions, previousResult);
        var inputPath = runtimeOptions.InvocationOptions.CniHost.GetTempFilePath();
        await runtimeOptions.InvocationOptions.CniHost.WriteFileAsync(inputPath, stdinJson, cancellationToken);
        
        var environment = new Dictionary<string, string> { { Constants.Environment.Command, operation } };
        if (runtimeOptions.PluginOptions.ContainerId is not null)
        {
            environment[Constants.Environment.ContainerId] = runtimeOptions.PluginOptions.ContainerId;
        }
        if (runtimeOptions.PluginOptions.InterfaceName is not null)
        {
            environment[Constants.Environment.InterfaceName] = runtimeOptions.PluginOptions.InterfaceName;
        }
        if (runtimeOptions.PluginOptions.NetworkNamespace is not null)
        {
            environment[Constants.Environment.NetworkNamespace] = runtimeOptions.PluginOptions.NetworkNamespace;
        }
        if (runtimeOptions.PluginSearchOptions.ActualDirectory is not null && runtimeOptions.PluginOptions.IncludePath)
        {
            environment[Constants.Environment.PluginPath] = runtimeOptions.PluginSearchOptions.ActualDirectory;
        }

        var process = await runtimeOptions.InvocationOptions.CniHost.StartProcessAsync(
            $"{pluginBinary} < {inputPath}", environment, runtimeOptions.InvocationOptions, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        await runtimeOptions.InvocationOptions.CniHost.DeleteFileAsync(inputPath, cancellationToken);
        
        return process.CurrentOutput;
    }
    
    private static string DerivePluginInput(Plugin plugin, RuntimeOptions runtimeOptions, AddCniResult? previousResult)
    {
        var jsonNode = plugin.PluginParameters.DeepClone();
        
        jsonNode[Constants.Parsing.CniVersion] = runtimeOptions.PluginOptions.CniVersion;
        jsonNode[Constants.Parsing.Name] = runtimeOptions.PluginOptions.Name;
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

    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9_.\-]*$")]
    private static partial Regex CniRegexGenerator();
}