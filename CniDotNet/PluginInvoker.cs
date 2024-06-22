using CniDotNet.Data;

namespace CniDotNet;

internal static class PluginInvoker
{
    public static async Task<string> InvokeAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string cniVersion,
        CancellationToken cancellationToken)
    {
        var stdinJson = NetworkPluginParser.SaveToStringInternal(networkPlugin, cniVersion);
        return "";
    }
}