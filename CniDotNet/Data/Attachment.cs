using CniDotNet.Data.Options;

namespace CniDotNet.Data;

public sealed record Attachment(
    PluginOptions PluginOptions,
    Plugin Plugin,
    PluginList? ParentPluginList);