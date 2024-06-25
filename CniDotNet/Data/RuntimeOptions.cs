namespace CniDotNet.Data;

public sealed record RuntimeOptions(
    PluginOptions PluginOptions,
    InvocationOptions InvocationOptions,
    PluginSearchOptions PluginSearchOptions);