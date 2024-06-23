using CniDotNet.Data;
using CniDotNet.Host;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var conf = await NetworkConfigurationParser.LookupFirstAsync(LocalCniHost.Current,
            new ConfigurationLookupOptions([".conflist"], Directory: "/etc/cni/net.d"));
        var firstPlugin = conf!.Plugins[0];
        var runtimeOptions = new RuntimeOptions(
            ContainerId: "fcnet",
            NetworkNamespace: "testing",
            InterfaceName: "eth0",
            CniVersion: "1.0.0",
            ElevationPassword: "",
            CniHost: LocalCniHost.Current);

        await CniRuntime.AddSinglePluginAsync(
            firstPlugin,
            runtimeOptions,
            new PluginLookupOptions(Directory: "/home/kanpov/plugins/bin"));
    }

    [Fact]
    public async Task Test2()
    {
        var proc = await LocalCniHost.Current.StartProcessWithElevationAsync(
            "/home/kanpov/plugins/bin/ptp", new Dictionary<string, string>
            {
                { "CNI_COMMAND", "VERSION" }
            }, "495762", "/bin/sudo", default);
        var output = await proc.WaitForExitAsync(default);
        Console.WriteLine(output);
    }
}