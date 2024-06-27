using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Data.Results;

namespace CniDotNet.Abstractions;

public interface IInvocationStore
{
    Task SetBinaryLocationAsync(string pluginType, string binaryLocation, CancellationToken cancellationToken);

    Task<string?> GetBinaryLocationAsync(string pluginType, CancellationToken cancellationToken);
    
    Task AddAttachmentAsync(Attachment attachment, CancellationToken cancellationToken);

    Task RemoveAttachmentAsync(Plugin plugin, PluginOptions pluginOptions, CancellationToken cancellationToken);
    
    Task<Attachment?> GetAttachmentAsync(Plugin plugin, PluginOptions pluginOptions, CancellationToken cancellationToken);

    Task<IReadOnlyList<Attachment>> GetAllAttachmentsForPluginAsync(Plugin plugin, CancellationToken cancellationToken);

    Task<IReadOnlyList<Attachment>> GetAllAttachmentsForPluginListAsync(PluginList pluginList, CancellationToken cancellationToken);

    Task SetResultAsync(PluginList pluginList, AddCniResult result, CancellationToken cancellationToken);

    Task<AddCniResult?> GetResultAsync(PluginList pluginList, CancellationToken cancellationToken);

    Task RemoveResultAsync(PluginList pluginList, CancellationToken cancellationToken);
}