using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Data.Results;

namespace CniDotNet.Abstractions;

public sealed class InMemoryInvocationStore : IInvocationStore
{
    public static readonly InMemoryInvocationStore Instance = new();
    
    private readonly Dictionary<string, string> _binaryLocationEntries = new();
    private readonly HashSet<Attachment> _attachmentEntries = [];
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

    public Task AddAttachmentAsync(Attachment attachment)
    {
        _attachmentEntries.Add(attachment);
        return Task.CompletedTask;
    }

    public Task RemoveAttachmentAsync(Plugin plugin, PluginOptions pluginOptions)
    {
        _attachmentEntries.RemoveWhere(a => a.Plugin == plugin && a.PluginOptions == pluginOptions);
        return Task.CompletedTask;
    }

    public Task<Attachment?> GetAttachmentAsync(Plugin plugin, PluginOptions pluginOptions)
    {
        return Task.FromResult(
            _attachmentEntries.FirstOrDefault(a => a.Plugin == plugin && a.PluginOptions == pluginOptions));
    }

    public Task<IReadOnlyList<Attachment>> GetAllAttachmentsForPluginAsync(Plugin plugin)
    {
        return Task.FromResult<IReadOnlyList<Attachment>>(
            _attachmentEntries.Where(a => a.Plugin == plugin).ToList());
    }

    public Task<IReadOnlyList<Attachment>> GetAllAttachmentsForPluginListAsync(PluginList pluginList)
    {
        return Task.FromResult<IReadOnlyList<Attachment>>(
            _attachmentEntries.Where(a => a.ParentPluginList == pluginList).ToList());
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

    public Task RemoveResultAsync(PluginList pluginList)
    {
        _resultEntries.Remove(pluginList);
        return Task.CompletedTask;
    }
}