using CniDotNet.Data;

namespace CniDotNet;

public static class CniRuntime
{
    private const string OperationAdd = "ADD";
    
    public static async Task AddSinglePluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        string cniVersion,
        CancellationToken cancellationToken = default)
    {
        await PluginInvoker.InvokeAsync(networkPlugin, runtimeOptions, OperationAdd, cniVersion, cancellationToken);
    }
}