using CniDotNet.Data;

namespace CniDotNet;

internal static class PluginInvoker
{
    public static async Task<string> InvokeAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        CancellationToken cancellationToken)
    {
        var stdinJson = NetworkPluginParser.SaveToStringInternal(networkPlugin, runtimeOptions.CniVersion!);
        return "";
    }
}