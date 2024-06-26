using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Data.Results;

namespace CniDotNet.Abstractions;

public sealed class InMemoryInvocationStore : IInvocationStore
{
    public static readonly InMemoryInvocationStore Instance = new();
    
    private readonly Dictionary<string, string> _binaryLocationEntries = new();
    private readonly HashSet<StoredAttachment> _attachmentEntries = [];
    private readonly Dictionary<PluginList, AddCniResult> _resultEntries = new();
    
    private InMemoryInvocationStore() {}
    
    public Task SetBinaryLocationAsync(string pluginType, string binaryLocation)
    {
        _binaryLocationEntries[pluginType] = binaryLocation;
        return Task.CompletedTask;
    }

    public Task<string?> GetBinaryLocationAsync(string pluginType)
    {
        return Task.FromResult<string?>(
            _binaryLocationEntries.FirstOrDefault(x => x.Key == pluginType).Value);
    }

    public Task AddAttachmentAsync(StoredAttachment storedAttachment)
    {
        _attachmentEntries.Add(storedAttachment);
        return Task.CompletedTask;
    }

    public Task RemoveAttachmentAsync(Plugin plugin, PluginOptions pluginOptions)
    {
        _attachmentEntries.RemoveWhere(a => a.Plugin == plugin && a.PluginOptions == pluginOptions);
        return Task.CompletedTask;
    }

    public Task<StoredAttachment?> GetAttachmentAsync(Plugin plugin, PluginOptions pluginOptions)
    {
        return Task.FromResult(
            _attachmentEntries.FirstOrDefault(a => a.Plugin == plugin && a.PluginOptions == pluginOptions));
    }

    public Task<IEnumerable<StoredAttachment>> GetAllAttachmentsForPluginAsync(Plugin plugin)
    {
        return Task.FromResult(
            _attachmentEntries.Where(a => a.Plugin == plugin));
    }

    public Task<IEnumerable<StoredAttachment>> GetAllAttachmentsForPluginListAsync(PluginList pluginList)
    {
        return Task.FromResult(
            _attachmentEntries.Where(a => a.ParentPluginList == pluginList));
    }

    public Task SetResultAsync(PluginList pluginList, AddCniResult result)
    {
        _resultEntries[pluginList] = result;
        return Task.CompletedTask;
    }

    public Task<AddCniResult?> GetResultAsync(PluginList pluginList)
    {
        return Task.FromResult(_resultEntries.GetValueOrDefault(pluginList));
    }
}