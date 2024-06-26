namespace CniDotNet.Data;

public sealed record Attachment(
    Plugin Plugin,
    PluginOptions PluginOptions,
    PluginList? Parent = null);