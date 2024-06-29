using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;

namespace CniDotNet.Abstractions;

public sealed class MemoryInvocationStore : IInvocationStore
{
    public static readonly MemoryInvocationStore Instance = new();
    
    private readonly Dictionary<string, string> _binaryLocationEntries = new();
    private readonly HashSet<Attachment> _attachmentEntries = [];
    private readonly Dictionary<PluginList, CniAddResult> _resultEntries = new();
    
    private MemoryInvocationStore() {}
    
    public Task SetBinaryLocationAsync(string pluginType, string binaryLocation, CancellationToken cancellationToken)
    {
        _binaryLocationEntries[pluginType] = binaryLocation;
        return Task.CompletedTask;
    }

    public Task<string?> GetBinaryLocationAsync(string pluginType, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(
            _binaryLocationEntries.FirstOrDefault(x => x.Key == pluginType).Value);
    }

    public Task AddAttachmentAsync(Attachment attachment, CancellationToken cancellationToken)
    {
        _attachmentEntries.Add(attachment);
        return Task.CompletedTask;
    }

    public Task RemoveAttachmentAsync(Plugin plugin, PluginOptions pluginOptions, CancellationToken cancellationToken)
    {
        _attachmentEntries.RemoveWhere(a => a.Plugin == plugin && a.PluginOptions == pluginOptions);
        return Task.CompletedTask;
    }

    public Task<Attachment?> GetAttachmentAsync(Plugin plugin, PluginOptions pluginOptions, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _attachmentEntries.FirstOrDefault(a => a.Plugin == plugin && a.PluginOptions == pluginOptions));
    }

    public Task<IEnumerable<Attachment>> GetAllAttachmentsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_attachmentEntries.AsEnumerable());
    }

    public Task<IEnumerable<Attachment>> GetAllAttachmentsForPluginAsync(Plugin plugin, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _attachmentEntries.Where(a => a.Plugin == plugin));
    }

    public Task<IEnumerable<Attachment>> GetAllAttachmentsForPluginListAsync(PluginList pluginList, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _attachmentEntries.Where(a => a.ParentPluginList == pluginList));
    }

    public Task SetResultAsync(PluginList pluginList, CniAddResult result, CancellationToken cancellationToken)
    {
        _resultEntries[pluginList] = result;
        return Task.CompletedTask;
    }

    public Task<CniAddResult?> GetResultAsync(PluginList pluginList, CancellationToken cancellationToken)
    {
        return Task.FromResult(_resultEntries.GetValueOrDefault(pluginList));
    }

    public Task RemoveResultAsync(PluginList pluginList, CancellationToken cancellationToken)
    {
        _resultEntries.Remove(pluginList);
        return Task.CompletedTask;
    }
}