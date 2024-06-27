using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Invocations;
using CniDotNet.Data.Options;
using CniDotNet.Runtime.Exceptions;

namespace CniDotNet.Runtime;

public static partial class CniRuntime
{
    private const PluginOptionRequirement AddRequirements = PluginOptionRequirement.ContainerId |
                                                            PluginOptionRequirement.InterfaceName |
                                                            PluginOptionRequirement.NetworkNamespace;
    private const PluginOptionRequirement DeleteRequirements = PluginOptionRequirement.ContainerId |
                                                              PluginOptionRequirement.InterfaceName;
    private const PluginOptionRequirement CheckRequirements = AddRequirements;
    private const PluginOptionRequirement ProbeVersionsRequirements = 0; // none
    private const PluginOptionRequirement VerifyReadinessRequirements = 0; // none
    private const PluginOptionRequirement GarbageCollectRequirements = PluginOptionRequirement.Path;
    private const string ErrorDetector = "\"code\": ";
    
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

    public static async Task<PluginListAddInvocation> AddPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (pluginList.Plugins.Count == 0) throw new CniEmptyPluginListException();
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);

        var attachments = new List<Attachment>();
        AddCniResult? previousAddResult = null;

        foreach (var plugin in pluginList.Plugins)
        {
            var invocation = await AddPluginInternalAsync(plugin, runtimeOptions, previousAddResult, pluginList,
                cancellationToken);
            
            if (invocation.IsError)
            {
                return PluginListAddInvocation.Error(invocation.ErrorResult!, plugin);
            }
            
            attachments.Add(invocation.SuccessAttachment!);
            previousAddResult = invocation.SuccessAddResult!;
        }
        
        return PluginListAddInvocation.Success(attachments, previousAddResult!);
    }

    public static async Task<PluginListInvocation> DeletePluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        AddCniResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);

        foreach (var plugin in pluginList.Plugins)
        {
            var invocation = await DeletePluginInternalAsync(plugin, runtimeOptions, lastAddResult, cancellationToken);
            
            if (invocation.IsError)
            {
                return PluginListInvocation.Error(invocation.ErrorResult!, plugin);
            }
        }
        
        return PluginListInvocation.Success;
    }

    public static async Task<PluginListInvocation> DeletePluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        PluginListAddInvocation pluginListAddInvocation,
        CancellationToken cancellationToken = default)
    {
        if (pluginListAddInvocation.IsError)
        {
            throw new ArgumentOutOfRangeException(nameof(pluginListAddInvocation), pluginListAddInvocation,
                "Invocation must be successful");
        }

        return await DeletePluginListAsync(pluginList, runtimeOptions, pluginListAddInvocation.SuccessAddResult!, cancellationToken);
    }

    public static async Task<PluginAddInvocation> AddPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);
        return await AddPluginInternalAsync(plugin, runtimeOptions, previousAddResult: null, pluginList: null, cancellationToken);
    }

    public static async Task<PluginInvocation> DeletePluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);
        return await DeletePluginInternalAsync(plugin, runtimeOptions, lastAddResult, cancellationToken);
    }

    public static async Task<PluginInvocation> DeletePluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        PluginAddInvocation pluginAddInvocation,
        CancellationToken cancellationToken = default)
    {
        if (pluginAddInvocation.IsError)
        {
            throw new ArgumentOutOfRangeException(nameof(pluginAddInvocation), pluginAddInvocation, "Invocation must be successful");
        }
        
        return await DeletePluginAsync(plugin, runtimeOptions, pluginAddInvocation.SuccessAddResult!, cancellationToken);
    } 
    
    private static async Task<PluginAddInvocation> AddPluginInternalAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult? previousAddResult = null,
        PluginList? pluginList = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, Constants.Operations.Add, pluginBinary,
            previousAddResult, gcAttachments: null, cancellationToken);

        if (resultJson.Contains(ErrorDetector))
        {
            var errorResult = JsonSerializer.Deserialize<ErrorCniResult>(resultJson, SerializerOptions);
            return PluginAddInvocation.Error(errorResult!);
        }

        var addResult = JsonSerializer.Deserialize<AddCniResult>(resultJson, SerializerOptions)!;
        var attachment = new Attachment(runtimeOptions.PluginOptions, plugin, pluginList);

        if (runtimeOptions.InvocationStoreOptions is { StoreAttachments: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.AddAttachmentAsync(attachment, cancellationToken);
        }
        
        return PluginAddInvocation.Success(attachment, addResult);
    }

    private static async Task<PluginInvocation> DeletePluginInternalAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = await SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await InvokeAsync(plugin, runtimeOptions, Constants.Operations.Delete, pluginBinary,
            lastAddResult, gcAttachments: null, cancellationToken);
        var invocation = MapResultJsonToPluginInvocation(resultJson);

        if (invocation.IsSuccess && runtimeOptions.InvocationStoreOptions is { StoreAttachments: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.RemoveAttachmentAsync(plugin,
                runtimeOptions.PluginOptions, cancellationToken);
        }

        return invocation;
    }

    private static PluginInvocation MapResultJsonToPluginInvocation(string resultJson)
    {
        if (!resultJson.Contains(ErrorDetector)) return PluginInvocation.Success;
        
        var errorResult = JsonSerializer.Deserialize<ErrorCniResult>(resultJson, SerializerOptions)!;
        return PluginInvocation.Error(errorResult);
    }

    private static async Task<AddCniResult> GetStoredResultAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreResults: true })
        {
            throw new CniStoreRetrievalException(
                "Store hasn't been configured for storing results, and yet a result is being requested");
        }

        var previousResult = await runtimeOptions.InvocationStoreOptions.InvocationStore.GetResultAsync(pluginList,
            cancellationToken);
        if (previousResult is null)
        {
            throw new CniStoreRetrievalException(
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
            throw new CniValidationFailureException(
                $"Path is required for \"{operation}\" but is excluded according to IncludePath=false");
        }
        
        // network namespace
        if (requirement.HasFlag(PluginOptionRequirement.NetworkNamespace) && string.IsNullOrWhiteSpace(pluginOptions.NetworkNamespace))
        {
            throw new CniValidationFailureException(
                $"Network namespace is required for \"{operation}\" but isn't provided");
        }
        
        // network name
        if (string.IsNullOrWhiteSpace(pluginOptions.Name))
        {
            throw new CniValidationFailureException("Network name is required for any operation but is missing");
        }

        if (!CniRegex.IsMatch(pluginOptions.Name))
        {
            throw new CniValidationFailureException($"Network name \"{pluginOptions.Name}\" doesn't match regex");
        }
        
        // container ID
        if (requirement.HasFlag(PluginOptionRequirement.ContainerId) && string.IsNullOrWhiteSpace(pluginOptions.ContainerId))
        {
            throw new CniValidationFailureException($"Container ID is required for \"{operation}\" but isn't provided");
        }
        
        if (pluginOptions.ContainerId is not null && !CniRegex.IsMatch(pluginOptions.ContainerId))
        {
            throw new CniValidationFailureException($"Container ID \"{pluginOptions.ContainerId}\" doesn't match regex");
        }
        
        // interface name
        if (requirement.HasFlag(PluginOptionRequirement.InterfaceName) && string.IsNullOrWhiteSpace(pluginOptions.InterfaceName))
        {
            throw new CniValidationFailureException($"Interface name is required for \"{operation}\" but isn't provided");
        }

        if (pluginOptions.InterfaceName is null) return;

        if (pluginOptions.InterfaceName.Length > MaximumInterfaceNameLength)
        {
            throw new CniValidationFailureException(
                $"Interface name \"{pluginOptions.InterfaceName}\" is longer than the maximum of {MaximumInterfaceNameLength}");
        }

        if (pluginOptions.InterfaceName is "." or "..")
        {
            throw new CniValidationFailureException("Interface name is either . or .., neither of which are allowed");
        }

        if (pluginOptions.InterfaceName.Any(c => c is '/' or ':' || char.IsWhiteSpace(c)))
        {
            throw new CniValidationFailureException(
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
                .GetBinaryLocationAsync(plugin.Type, cancellationToken);
            if (hitLocation is not null) return hitLocation;
        }
        
        var matchFromTable = runtimeOptions.PluginSearchOptions.SearchTable?.GetValueOrDefault(plugin.Type);
        if (matchFromTable is not null) return matchFromTable;

        var directory = await runtimeOptions.PluginSearchOptions.GetActualDirectoryAsync(
                runtimeOptions.InvocationOptions.RuntimeHost, cancellationToken);
        if (directory is null)
        {
            throw new CniBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: directory wasn't specified and " +
                                              $"environment variable doesn't exist");
        }

        if (!runtimeOptions.InvocationOptions.RuntimeHost.DirectoryExists(directory))
        {
            throw new CniBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: \"{directory}\" directory " +
                                              $"doesn't exist");
        }

        var matchingFiles = await runtimeOptions.InvocationOptions.RuntimeHost.EnumerateDirectoryAsync(
            directory, plugin.Type, runtimeOptions.PluginSearchOptions.DirectorySearchOption,
            cancellationToken);
        var missLocation = matchingFiles.FirstOrDefault();
        if (missLocation is null)
        {
            throw new CniBinaryNotFoundException(
                $"Could not find \"{plugin.Type}\" plugin: the file doesn't exist according to the given search option" +
                $"in the \"{directory}\" directory");
        }

        if (usesCache)
        {
            await runtimeOptions.InvocationStoreOptions!.InvocationStore.SetBinaryLocationAsync(
                plugin.Type, missLocation, cancellationToken);
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