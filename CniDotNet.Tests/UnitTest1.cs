using CniDotNet.Abstractions;
using CniDotNet.Data;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var conf = await NetworkConfigurationParser.LookupFirstAsync(LocalFilesystem.Current,
            new LookupOptions([".conflist"], Directory: "/etc/cni/net.d"));
        var firstPlugin = conf!.Plugins[0];
        var runtimeOptions = new RuntimeOptions(
            ContainerId: "fcnet",
            NetworkNamespace: "testing",
            InterfaceName: "eth0");

        await CniRuntime.AddSinglePluginAsync(firstPlugin, runtimeOptions, "1.0.0");
    }
}