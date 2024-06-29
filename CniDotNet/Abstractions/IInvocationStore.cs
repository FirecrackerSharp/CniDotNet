using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;

namespace CniDotNet.Abstractions;

public interface IInvocationStore
{
    Task ClearAsync(CancellationToken cancellationToken);
    
    Task SetBinaryLocationAsync(string pluginType, string binaryLocation, CancellationToken cancellationToken);

    Task<string?> GetBinaryLocationAsync(string pluginType, CancellationToken cancellationToken);
    
    Task AddAttachmentAsync(Attachment attachment, CancellationToken cancellationToken);

    Task RemoveAttachmentAsync(Plugin plugin, PluginOptions pluginOptions, CancellationToken cancellationToken);
    
    Task<Attachment?> GetAttachmentAsync(Plugin plugin, PluginOptions pluginOptions, CancellationToken cancellationToken);

    Task<IEnumerable<Attachment>> GetAllAttachmentsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<Attachment>> GetAllAttachmentsForPluginAsync(Plugin plugin, CancellationToken cancellationToken);

    Task<IEnumerable<Attachment>> GetAllAttachmentsForPluginListAsync(PluginList pluginList, CancellationToken cancellationToken);

    Task SetResultAsync(PluginList pluginList, CniAddResult result, CancellationToken cancellationToken);

    Task<CniAddResult?> GetResultAsync(PluginList pluginList, CancellationToken cancellationToken);

    Task RemoveResultAsync(PluginList pluginList, CancellationToken cancellationToken);
}