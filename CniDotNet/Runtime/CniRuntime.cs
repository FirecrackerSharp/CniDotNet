using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Data.Results;

namespace CniDotNet.Runtime;

public static partial class CniRuntime
{
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
    
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverterWithAttributeSupport() }
    };

    internal static readonly JsonSerializerOptions PrettyPrintSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverterWithAttributeSupport() },
        WriteIndented = true
    };
    
    public static async Task<WrappedCniResult<AddCniResult>> AddPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);
        AddCniResult? previousResult = null;

        foreach (var plugin in pluginList.Plugins)
        {
            var wrappedResult = await AddPluginAsync(
                plugin, runtimeOptions, previousResult, pluginList, cancellationToken);
            previousResult = wrappedResult.SuccessValue;
            
            if (wrappedResult.IsError)
            {
                return WrappedCniResult<AddCniResult>.Error(wrappedResult.ErrorValue!);
            }
        }

        if (runtimeOptions.InvocationStoreOptions is { StoreResults: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.SetResultAsync(pluginList, previousResult!);
        }
        
        return WrappedCniResult<AddCniResult>.Success(previousResult!);
    }

    public static async Task<ErrorCniResult?> DeletePluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);
        
        for (var i = pluginList.Plugins.Count - 1; i >= 0; i--)
        {
            var plugin = pluginList.Plugins[i];
            var errorCniResult = await DeletePluginAsync(
                plugin, runtimeOptions, previousResult, pluginList, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        if (runtimeOptions.InvocationStoreOptions is { StoreResults: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.RemoveResultAsync(pluginList);
        }

        return null;
    }

    public static async Task<ErrorCniResult?> DeletePluginListWithStoreAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        return await DeletePluginListAsync(
            pluginList,
            runtimeOptions,
            await GetStoredResultAsync(pluginList, runtimeOptions, cancellationToken),
            cancellationToken);
    }

    public static async Task<ErrorCniResult?> CheckPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        CancellationToken cancellationToken = default)
    {
        if (pluginList.DisableCheck) return null;
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Check, CheckRequirements);
        
        foreach (var plugin in pluginList.Plugins)
        {
            var errorCniResult = await CheckPluginAsync(plugin, runtimeOptions, previousResult, pluginList, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> CheckPluginListWithStoreAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        return await CheckPluginListAsync(
            pluginList,
            runtimeOptions,
            await GetStoredResultAsync(pluginList, runtimeOptions, cancellationToken),
            cancellationToken);
    }

    public static async Task<ErrorCniResult?> VerifyPluginListReadinessAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.VerifyReadiness, VerifyReadinessRequirements);
        
        foreach (var plugin in pluginList.Plugins)
        {
            var errorCniResult = await VerifyPluginReadinessAsync(plugin, runtimeOptions, pluginList, cancellationToken);
            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> GarbageCollectPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        IReadOnlyList<Attachment> gcAttachments,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.GarbageCollect, GarbageCollectRequirements);

        foreach (var plugin in pluginList.Plugins)
        {
            var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
            var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.GarbageCollect,
                pluginBinary, previousResult: null, gcAttachments, cancellationToken);
            var errorCniResult = WrapPotentialErrorCniResult(resultJson);

            if (errorCniResult is not null) return errorCniResult;
        }

        return null;
    }

    public static async Task<ErrorCniResult?> GarbageCollectPluginListWithStoreAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreAttachments: true })
        {
            throw new ItemNotRetrievedFromStoreException("Store hasn't been configured for storing attachments, yet an attachment is needed");
        }

        var gcAttachments =
            await runtimeOptions.InvocationStoreOptions.InvocationStore.GetAllAttachmentsForPluginListAsync(pluginList);
        return await GarbageCollectPluginListAsync(pluginList, runtimeOptions, gcAttachments, cancellationToken);
    }

    public static async Task<WrappedCniResult<AddCniResult>> AddPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult? previousResult = null,
        PluginList? pluginList = null,
        CancellationToken cancellationToken = default)
    {
        if (pluginList is null)
        {
            ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);
        }

        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Add,
            pluginBinary, previousResult, gcAttachments: null, cancellationToken);

        var wrappedResult = WrapCniResult<AddCniResult>(resultJson);
        if (wrappedResult.IsError) return wrappedResult;
        
        var attachment = new Attachment(runtimeOptions.PluginOptions, plugin, pluginList);
        wrappedResult.Attachment = attachment;
        if (runtimeOptions.InvocationStoreOptions is { StoreAttachments: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.AddAttachmentAsync(attachment);
        }
        
        return wrappedResult;
    }
    
    public static async Task<ErrorCniResult?> DeletePluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult? previousResult = null,
        PluginList? pluginList = null,
        CancellationToken cancellationToken = default)
    {
        if (pluginList is null)
        {
            ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);
        }

        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Delete,
            pluginBinary, previousResult, gcAttachments: null, cancellationToken);
        
        var errorResult = WrapPotentialErrorCniResult(resultJson);

        // if deleted successfully, remove stored attachment
        if (errorResult is null && runtimeOptions.InvocationStoreOptions is { StoreAttachments: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.RemoveAttachmentAsync(
                plugin, runtimeOptions.PluginOptions);
        }

        return errorResult;
    }

    public static async Task<ErrorCniResult?> CheckPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult previousResult,
        PluginList? pluginList = null,
        CancellationToken cancellationToken = default)
    {
        if (pluginList is null)
        {
            ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Check, CheckRequirements);
        }

        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.Check,
            pluginBinary, previousResult, gcAttachments: null, cancellationToken);
        
        return WrapPotentialErrorCniResult(resultJson);
    }

    public static async Task<ErrorCniResult?> VerifyPluginReadinessAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        PluginList? pluginList = null,
        CancellationToken cancellationToken = default)
    {
        if (pluginList is null)
        {
            ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.VerifyReadiness,
                VerifyReadinessRequirements);
        }

        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.VerifyReadiness,
            pluginBinary, previousResult: null, gcAttachments: null, cancellationToken);
        return WrapPotentialErrorCniResult(resultJson);
    }
    
    public static async Task<WrappedCniResult<VersionCniResult>> ProbePluginVersionsAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        PluginList? pluginList = null,
        CancellationToken cancellationToken = default)
    {
        if (pluginList is null)
        {
            ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.ProbeVersions,
                ProbeVersionsRequirements);
        }

        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, operation: Constants.Operations.ProbeVersions,
            pluginBinary, previousResult: null, gcAttachments: null, cancellationToken);
        return WrapCniResult<VersionCniResult>(resultJson);
    }

    private static WrappedCniResult<T> WrapCniResult<T>(string resultJson) where T : class
    {
        if (resultJson.Contains("\"code\": "))
        {
            var errorValue = JsonSerializer.Deserialize<ErrorCniResult>(resultJson);
            return WrappedCniResult<T>.Error(errorValue!);
        }

        var successValue = JsonSerializer.Deserialize<T>(resultJson);
        return WrappedCniResult<T>.Success(successValue!, resultJson);
    }

    private static ErrorCniResult? WrapPotentialErrorCniResult(string resultJson)
    {
        return string.IsNullOrWhiteSpace(resultJson)
            ? null
            : JsonSerializer.Deserialize<ErrorCniResult>(resultJson);
    }
    
    private static async Task<AddCniResult> GetStoredResultAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreResults: true })
        {
            throw new ItemNotRetrievedFromStoreException(
                "Store hasn't been configured for storing results, and yet a result is being requested");
        }

        var previousResult = await runtimeOptions.InvocationStoreOptions.InvocationStore.GetResultAsync(pluginList);
        if (previousResult is null)
        {
            throw new ItemNotRetrievedFromStoreException(
                $"No result has been stored for given plugin list (hash code {pluginList.GetHashCode()})");
        }

        return previousResult;
    }

    private static void ValidatePluginOptions(PluginOptions pluginOptions, string operation,
        PluginOptionRequirement requirement)
    {
        if (pluginOptions.SkipValidation) return;
        
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
        var usesCache = runtimeOptions.InvocationStoreOptions is { StoreBinaryLocations: true };
        if (usesCache)
        {
            var hitLocation = await runtimeOptions.InvocationStoreOptions!.InvocationStore
                .GetBinaryLocationAsync(plugin.Type);
            if (hitLocation is not null) return hitLocation;
        }
        
        var matchFromTable = runtimeOptions.PluginSearchOptions.SearchTable?.GetValueOrDefault(plugin.Type);
        if (matchFromTable is not null) return matchFromTable;

        var directory = await runtimeOptions.PluginSearchOptions.GetActualDirectoryAsync(
                runtimeOptions.InvocationOptions.RuntimeHost, cancellationToken);
        if (directory is null)
        {
            throw new PluginBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: directory wasn't specified and " +
                                              $"environment variable doesn't exist");
        }

        if (!runtimeOptions.InvocationOptions.RuntimeHost.DirectoryExists(directory))
        {
            throw new PluginBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: \"{directory}\" directory " +
                                              $"doesn't exist");
        }

        var matchingFiles = await runtimeOptions.InvocationOptions.RuntimeHost.EnumerateDirectoryAsync(
            directory, plugin.Type, runtimeOptions.PluginSearchOptions.DirectorySearchOption,
            cancellationToken);
        var missLocation = matchingFiles.FirstOrDefault();
        if (missLocation is null)
        {
            throw new PluginBinaryNotFoundException(
                $"Could not find \"{plugin.Type}\" plugin: the file doesn't exist according to the given search option" +
                $"in the \"{directory}\" directory");
        }

        if (usesCache)
        {
            await runtimeOptions.InvocationStoreOptions!.InvocationStore.SetBinaryLocationAsync(
                plugin.Type, missLocation);
        }

        return missLocation;
    }

    private static async Task<string> InvokeAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        AddCniResult? previousResult,
        IReadOnlyList<Attachment>? gcAttachments,
        CancellationToken cancellationToken)
    {
        var stdinJson = DerivePluginInput(plugin, runtimeOptions, previousResult, gcAttachments);
        
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
        if (runtimeOptions.PluginSearchOptions.CachedActualDirectory is not null && runtimeOptions.PluginOptions.IncludePath)
        {
            environment[Constants.Environment.PluginPath] = runtimeOptions.PluginSearchOptions.CachedActualDirectory;
        }

        var process = await runtimeOptions.InvocationOptions.RuntimeHost.StartProcessAsync(
            $"{pluginBinary} <<< '{stdinJson}'", environment, runtimeOptions.InvocationOptions, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return process.CurrentOutput;
    }
    
    private static string DerivePluginInput(Plugin plugin, RuntimeOptions runtimeOptions, AddCniResult? previousResult,
        IReadOnlyList<Attachment>? gcAttachments)
    {
        var jsonNode = plugin.PluginParameters.DeepClone();
        
        jsonNode[Constants.Parsing.CniVersion] = runtimeOptions.PluginOptions.CniVersion;
        jsonNode[Constants.Parsing.Name] = runtimeOptions.PluginOptions.Name;
        jsonNode[Constants.Parsing.Type] = plugin.Type;

        if (plugin.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] = plugin.Capabilities.DeepClone();
        }

        if (plugin.Args is not null)
        {
            jsonNode[Constants.Parsing.Args] = plugin.Args.DeepClone();
        }

        var extraCapabilities = runtimeOptions.PluginOptions.ExtraCapabilities;
        if (extraCapabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] ??= new JsonObject();
            foreach (var (capabilityKey, capabilityValue) in extraCapabilities)
            {
                if (capabilityValue is null) continue;
                if (!jsonNode[Constants.Parsing.RuntimeConfig]!.AsObject().ContainsKey(capabilityKey))
                {
                    jsonNode[Constants.Parsing.RuntimeConfig]![capabilityKey] = capabilityValue.DeepClone();
                }
            }
        }

        if (previousResult is not null)
        {
            jsonNode[Constants.Parsing.PreviousResult] = JsonSerializer
                .SerializeToNode(previousResult, SerializerOptions)!.AsObject();
        }

        if (gcAttachments is not null)
        {
            var jsonArray = new JsonArray();

            foreach (var gcAttachment in gcAttachments)
            {
                jsonArray.Add(new JsonObject
                {
                    [Constants.Parsing.GcContainerId] = gcAttachment.PluginOptions.ContainerId!,
                    [Constants.Parsing.GcInterfaceName] = gcAttachment.PluginOptions.InterfaceName!
                });
            }

            jsonNode[Constants.Parsing.GcAttachments] = jsonArray;
        }

        return JsonSerializer.Serialize(jsonNode, SerializerOptions);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9_.\-]*$")]
    private static partial Regex CniRegexGenerator();
}