using CniDotNet.Data;
using CniDotNet.Host;
using CniDotNet.Runtime;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var conf = await NetworkConfigurationLoader.LookupFirstAsync(LocalCniHost.Current,
            new ConfigurationLookupOptions([".conflist"], Directory: "/etc/cni/net.d"));
        var runtimeOptions = RuntimeOptions.FromConfiguration(
            conf!,
            containerId: "fcnet",
            networkNamespace: "/var/run/netns/testing",
            interfaceName: "eth0",
            pluginPath: "/home/kanpov/plugins/bin",
            elevationPassword: "495762",
            cniHost: LocalCniHost.Current);

        var plo = new PluginLookupOptions("/home/kanpov/plugins/bin");
        var wrappedResult = await CniRuntime.AddConfigurationAsync(
            conf!, runtimeOptions, pluginLookupOptions: plo);
        var previousResult = wrappedResult.SuccessValue!;

        var checkErrorResult = await CniRuntime.CheckConfigurationAsync(
            conf!, runtimeOptions, previousResult, pluginLookupOptions: plo);
        Assert.Null(checkErrorResult);

        var gcResult = await CniRuntime.GarbageCollectConfigurationAsync(
            conf!, runtimeOptions, pluginLookupOptions: plo);
        
        for (var i = 0; i < 2; ++i)
        {
            var deleteErrorResult = await CniRuntime.DeleteConfigurationAsync(
                conf!, runtimeOptions, previousResult,
                pluginLookupOptions: plo);
            Assert.Null(deleteErrorResult);
        }
    }
}