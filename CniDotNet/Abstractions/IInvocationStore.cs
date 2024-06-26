using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Data.Results;

namespace CniDotNet.Abstractions;

public interface IInvocationStore
{
    Task SetBinaryLocationAsync(string pluginType, string binaryLocation);

    Task<string?> GetBinaryLocationAsync(string pluginType);
    
    Task AddAttachmentAsync(StoredAttachment storedAttachment);

    Task RemoveAttachmentAsync(Plugin plugin, PluginOptions pluginOptions);
    
    Task<StoredAttachment?> GetAttachmentAsync(Plugin plugin, PluginOptions pluginOptions);

    Task<IEnumerable<StoredAttachment>> GetAllAttachmentsForPluginAsync(Plugin plugin);

    Task<IEnumerable<StoredAttachment>> GetAllAttachmentsForPluginListAsync(PluginList pluginList);

    Task SetResultAsync(PluginList pluginList, AddCniResult result);

    Task<AddCniResult?> GetResultAsync(PluginList pluginList);
}