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
    private const PluginOptionRequirement VersionRequirements = 0; // none
    private const PluginOptionRequirement StatusRequirements = 0; // none
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
        CniBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);

        var attachments = new List<Attachment>();
        CniAddResult? previousAddResult = null;

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
        CniAddResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        CniBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);

        for (var i = pluginList.Plugins.Count - 1; i >= 0; i--)
        {
            var plugin = pluginList.Plugins[i];
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
            await GetStoredResultAsync(pluginList, runtimeOptions, cancellationToken)
                ?? throw new CniStoreRetrievalException("Could not retrieve stored result"),
            cancellationToken);
    }

    public static async Task<PluginListVersionInvocation> VersionPluginListAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        CniBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Version, VersionRequirements);
        var versionResults = new Dictionary<Plugin, CniVersionResult>();

        foreach (var plugin in pluginList.Plugins)
        {
            var invocation = await VersionPluginInternalAsync(plugin, runtimeOptions, cancellationToken);

            if (invocation.IsError)
            {
                return PluginListVersionInvocation.Error(invocation.ErrorResult!, plugin);
            }

            versionResults[plugin] = invocation.SuccessVersionResult!;
        }
        
        return PluginListVersionInvocation.Success(versionResults);
    }

    public static Task<PluginListInvocation> GarbageCollectAsync(
        IEnumerable<Plugin> plugins,
        IEnumerable<Attachment> validAttachments,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default) =>
        GarbageCollectInternalAsync(plugins, runtimeOptions, validAttachments, cancellationToken);

    public static Task<PluginListInvocation> GarbageCollectAsync(
        PluginList pluginList,
        IEnumerable<Attachment> validAttachments,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default) =>
        GarbageCollectInternalAsync(pluginList.Plugins, runtimeOptions, validAttachments, cancellationToken);

    public static async Task<PluginListInvocation> GarbageCollectAsync(
        IEnumerable<PluginList> pluginLists,
        IEnumerable<Attachment> validAttachments,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var plugins = pluginLists.SelectMany(l => l.Plugins).DistinctBy(p => p.Type);
        return await GarbageCollectInternalAsync(plugins, runtimeOptions, validAttachments, cancellationToken);
    }

    public static async Task<PluginListInvocation> GarbageCollectWithStoreAsync(
        IEnumerable<Plugin> plugins,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var attachments = await GetAllStoredAttachmentsAsync(runtimeOptions, cancellationToken)
                          ?? throw new CniStoreRetrievalException("Could not get stored attachments");
        return await GarbageCollectInternalAsync(plugins, runtimeOptions, attachments, cancellationToken);
    }

    public static async Task<PluginListInvocation> GarbageCollectWithStoreAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var attachments = await GetAllStoredAttachmentsAsync(runtimeOptions, cancellationToken)
                          ?? throw new CniStoreRetrievalException("Could not get stored attachments");
        return await GarbageCollectInternalAsync(pluginList.Plugins, runtimeOptions, attachments, cancellationToken);
    }

    public static async Task<PluginListInvocation> GarbageCollectWithStoreAsync(
        IEnumerable<PluginList> pluginLists,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var plugins = pluginLists.SelectMany(l => l.Plugins).DistinctBy(p => p.Type);
        var attachments = await GetAllStoredAttachmentsAsync(runtimeOptions, cancellationToken)
                          ?? throw new CniStoreRetrievalException("Could not get stored attachments");
        return await GarbageCollectInternalAsync(plugins, runtimeOptions, attachments, cancellationToken);
    }

    public static async Task<PluginAddInvocation> AddPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        CniBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Add, AddRequirements);
        return await AddPluginInternalAsync(plugin, runtimeOptions, previousAddResult: null, pluginList: null, cancellationToken);
    }

    public static async Task<PluginInvocation> DeletePluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CniAddResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        CniBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Delete, DeleteRequirements);
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

    public static async Task<PluginVersionInvocation> VersionPluginAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        CniBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.Version, VersionRequirements);
        return await VersionPluginInternalAsync(plugin, runtimeOptions, cancellationToken);
    }

    public static async Task<CniAddResult?> GetStoredResultAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreResults: true }) return null;

        var result = await runtimeOptions.InvocationStoreOptions.InvocationStore.GetResultAsync(pluginList, cancellationToken);
        return result;
    }

    public static async Task<Attachment?> GetStoredAttachmentAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreAttachments: true }) return null;

        var attachment = await runtimeOptions.InvocationStoreOptions.InvocationStore.GetAttachmentAsync(plugin,
                runtimeOptions.PluginOptions, cancellationToken);
        return attachment;
    }

    public static async Task<IEnumerable<Attachment>?> GetStoredAttachmentsAsync(
        PluginList pluginList,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreAttachments: true }) return null;

        var attachments = await runtimeOptions.InvocationStoreOptions.InvocationStore
            .GetAllAttachmentsForPluginListAsync(pluginList, cancellationToken);
        return attachments;
    }

    public static async Task<IEnumerable<Attachment>?> GetStoredAttachmentsAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreAttachments: true }) return null;

        var attachments = await runtimeOptions.InvocationStoreOptions.InvocationStore
            .GetAllAttachmentsForPluginAsync(plugin, cancellationToken);
        return attachments;
    }

    public static async Task<IEnumerable<Attachment>?> GetAllStoredAttachmentsAsync(
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreAttachments: true }) return null;

        var attachments =
            await runtimeOptions.InvocationStoreOptions.InvocationStore.GetAllAttachmentsAsync(cancellationToken);
        return attachments;
    }

    public static async Task<string?> GetStoredBinaryLocationAsync(
        string pluginType,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        if (runtimeOptions.InvocationStoreOptions is not { StoreBinaryLocations: true }) return null;

        var binaryLocation = await runtimeOptions.InvocationStoreOptions.InvocationStore
            .GetBinaryLocationAsync(pluginType, cancellationToken);
        return binaryLocation;
    }

    public static Task<string?> GetStoredBinaryLocationAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default) =>
        GetStoredBinaryLocationAsync(plugin.Type, runtimeOptions, cancellationToken);
    
    private static async Task<PluginAddInvocation> AddPluginInternalAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CniAddResult? previousAddResult = null,
        PluginList? pluginList = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = await CniBackend.SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await CniBackend.InvokeAsync(plugin, runtimeOptions, Constants.Operations.Add, pluginBinary,
            previousAddResult, validAttachments: null, cancellationToken);

        if (resultJson.Contains(ErrorDetector))
        {
            var errorResult = JsonSerializer.Deserialize<CniErrorResult>(resultJson, SerializerOptions);
            return PluginAddInvocation.Error(errorResult!);
        }

        var addResult = JsonSerializer.Deserialize<CniAddResult>(resultJson, SerializerOptions)!;
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
        CniAddResult lastAddResult,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = await CniBackend.SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await CniBackend.InvokeAsync(plugin, runtimeOptions, Constants.Operations.Delete,
            pluginBinary, lastAddResult, validAttachments: null, cancellationToken);
        var invocation = MapResultJsonToPluginInvocation(resultJson);

        if (invocation.IsSuccess && runtimeOptions.InvocationStoreOptions is { StoreAttachments: true })
        {
            await runtimeOptions.InvocationStoreOptions.InvocationStore.RemoveAttachmentAsync(plugin,
                runtimeOptions.PluginOptions, cancellationToken);
        }

        return invocation;
    }

    private static async Task<PluginVersionInvocation> VersionPluginInternalAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = await CniBackend.SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
        var resultJson = await CniBackend.InvokeAsync(plugin, runtimeOptions, Constants.Operations.Version,
            pluginBinary, addResult: null, validAttachments: null, cancellationToken);

        if (resultJson.Contains(ErrorDetector))
        {
            var errorResult = JsonSerializer.Deserialize<CniErrorResult>(resultJson, SerializerOptions);
            return PluginVersionInvocation.Error(errorResult!);
        }

        var versionResult = JsonSerializer.Deserialize<CniVersionResult>(resultJson, SerializerOptions);
        return PluginVersionInvocation.Success(versionResult!);
    }

    private static async Task<PluginListInvocation> GarbageCollectInternalAsync(
        IEnumerable<Plugin> plugins,
        RuntimeOptions runtimeOptions,
        IEnumerable<Attachment> validAttachments,
        CancellationToken cancellationToken = default)
    {
        CniBackend.ValidatePluginOptions(runtimeOptions.PluginOptions, Constants.Operations.GarbageCollect,
            GarbageCollectRequirements);
        
        var enumeratedAttachments = validAttachments.ToList();
        
        foreach (var plugin in plugins)
        {
            var pluginBinary = await CniBackend.SearchForPluginBinaryAsync(plugin, runtimeOptions, cancellationToken);
            var resultJson = await CniBackend.InvokeAsync(plugin, runtimeOptions, Constants.Operations.GarbageCollect,
                pluginBinary, addResult: null, enumeratedAttachments, cancellationToken);
            if (!resultJson.Contains(ErrorDetector)) continue;
            
            var errorResult = JsonSerializer.Deserialize<CniErrorResult>(resultJson, SerializerOptions);
            if (errorResult!.Message.Contains("unknown CNI_COMMAND")) continue; // plugin doesn't support GC
            return PluginListInvocation.Error(errorResult, plugin);
        }
        
        return PluginListInvocation.Success;
    }

    private static PluginInvocation MapResultJsonToPluginInvocation(string resultJson)
    {
        if (!resultJson.Contains(ErrorDetector)) return PluginInvocation.Success;
        
        var errorResult = JsonSerializer.Deserialize<CniErrorResult>(resultJson, SerializerOptions)!;
        return PluginInvocation.Error(errorResult);
    }
}