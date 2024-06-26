using CniDotNet.Data.Options;

namespace CniDotNet.Data;

public sealed record StoredAttachment(
    PluginOptions PluginOptions,
    Plugin Plugin,
    PluginList? ParentPluginList);