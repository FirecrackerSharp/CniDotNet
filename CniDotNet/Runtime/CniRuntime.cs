using System.Text.Json;
using System.Text.Json.Serialization;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Invocations;
using CniDotNet.Data.Options;
using CniDotNet.Runtime.Exceptions;

namespace CniDotNet.Runtime;

public static class CniRuntime
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
        CniRuntimeBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);

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

        if (runtimeOptions.InvocationStoreOptions is { StoreResults: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.SetResultAsync(pluginList, previousAddResult!,
                cancellationToken);
        }
        
        return PluginListAddInvocation.Success(attachments, previousAddResult!);
    }

    public static async Task<PluginListInvocation> DeletePluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        AddCniResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        CniRuntimeBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);

        foreach (var plugin in pluginList.Plugins)
        {
            var invocation = await DeletePluginInternalAsync(plugin, runtimeOptions, lastAddResult, cancellationToken);
            
            if (invocation.IsError)
            {
                return PluginListInvocation.Error(invocation.ErrorResult!, plugin);
            }
        }

        if (runtimeOptions.InvocationStoreOptions is { StoreResults: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.RemoveResultAsync(pluginList, cancellationToken);
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

    public static async Task<PluginListInvocation> DeletePluginListWithStoredResultAsync(
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

    public static async Task<PluginAddInvocation> AddPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        CniRuntimeBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);
        return await AddPluginInternalAsync(plugin, runtimeOptions, previousAddResult: null, pluginList: null, cancellationToken);
    }

    public static async Task<PluginInvocation> DeletePluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        AddCniResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        CniRuntimeBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);
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
        var pluginBinary = await CniRuntimeBackend.SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await CniRuntimeBackend.InvokeAsync(plugin, runtimeOptions, Constants.Operations.Add, pluginBinary,
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
        var pluginBinary = await CniRuntimeBackend.SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await CniRuntimeBackend.InvokeAsync(plugin, runtimeOptions, Constants.Operations.Delete,
            pluginBinary, lastAddResult, gcAttachments: null, cancellationToken);
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
}