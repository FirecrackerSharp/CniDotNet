using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Data.Results;

namespace CniDotNet.Abstractions;

public interface IInvocationStore
{
    Task SetBinaryLocationAsync(string pluginType, string binaryLocation);

    Task<string?> GetBinaryLocationAsync(string pluginType);
    
    Task AddAttachmentAsync(Attachment attachment);

    Task RemoveAttachmentAsync(Plugin plugin, PluginOptions pluginOptions);
    
    Task<Attachment?> GetAttachmentAsync(Plugin plugin, PluginOptions pluginOptions);

    Task<IReadOnlyList<Attachment>> GetAllAttachmentsForPluginAsync(Plugin plugin);

    Task<IReadOnlyList<Attachment>> GetAllAttachmentsForPluginListAsync(PluginList pluginList);

    Task SetResultAsync(PluginList pluginList, AddCniResult result);

    Task<AddCniResult?> GetResultAsync(PluginList pluginList);
}