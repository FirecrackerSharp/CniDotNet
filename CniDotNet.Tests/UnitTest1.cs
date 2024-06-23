using CniDotNet.Data;
using CniDotNet.Host;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var conf = await NetworkConfigurationLoader.LookupFirstAsync(LocalCniHost.Current,
            new ConfigurationLookupOptions([".conflist"], Directory: "/etc/cni/net.d"));
        var firstPlugin = conf!.Plugins[0];
        var runtimeOptions = new RuntimeOptions(
            ContainerId: "fcnet",
            NetworkNamespace: "/var/run/netns/testing",
            InterfaceName: "eth0",
            CniVersion: "1.0.0",
            PluginPath: "/home/kanpov/plugins/bin",
            ElevationPassword: "495762",
            CniHost: LocalCniHost.Current);

        var o = await CniRuntime.ProbePluginVersionsAsync(
            firstPlugin,
            runtimeOptions,
            pluginLookupOptions: new PluginLookupOptions(Directory: "/home/kanpov/plugins/bin"));
        Console.WriteLine(o);
    }
}