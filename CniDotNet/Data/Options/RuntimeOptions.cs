namespace CniDotNet.Data.Options;

public sealed record RuntimeOptions(
    PluginOptions PluginOptions,
    InvocationOptions InvocationOptions,
    PluginSearchOptions PluginSearchOptions,
    InvocationStoreOptions? InvocationStoreOptions = null);