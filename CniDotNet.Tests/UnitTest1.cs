using CniDotNet.Host.Local;
using CniDotNet.Data;
using CniDotNet.Host;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var ptp = new PtpPlugin(
            new HostLocalIpam(
                ResolvConf: "/etc/resolv.conf",
                Ranges: [[new HostLocalIpamRange(Subnet: "192.168.127.0/24")]]),
            IpMasq: true);
        var firewall = new FirewallPlugin();
        var tcRedirectTap = new TcRedirectTapPlugin();

        var typedPluginList = new TypedPluginList(
            CniVersion: new Version(1, 0, 0),
            Name: "fcnet",
            [ptp, firewall, tcRedirectTap]);
        var pluginList = typedPluginList.Build();
        
        var cniRuntimeOptions = RuntimeOptions.FromNetworkList(
            pluginList,
            containerId: "fcnet",
            networkNamespace: "/var/run/netns/testing",
            interfaceName: "eth0",
            new InvocationOptions(LocalCniHost.Current, "495762"),
            new PluginSearchOptions(Directory: "/home/kanpov/plugins/bin"));
        
        var wrappedResult = await CniRuntime.AddPluginListAsync(pluginList, cniRuntimeOptions);
        var previousResult = wrappedResult.SuccessValue!;

        var res = await CniRuntime.ProbePluginListVersionsAsync(pluginList, cniRuntimeOptions);

        var checkErrorResult = await CniRuntime.CheckPluginListAsync(pluginList, cniRuntimeOptions, previousResult);
        Assert.Null(checkErrorResult);

        for (var i = 0; i < 1; ++i)
        {
            var deleteErrorResult = await CniRuntime.DeletePluginListAsync(pluginList, cniRuntimeOptions, previousResult);
            Assert.Null(deleteErrorResult);
        }
    }

    [Fact]
    public async Task Test2()
    {
        var invocationOptions = new InvocationOptions(
            LocalCniHost.Current, "495762");
        
        var namespaces = await NetworkNamespaces.GetAllAsync(invocationOptions);
        Console.WriteLine(namespaces);

        var error = await NetworkNamespaces.DeleteAsync("m", invocationOptions);
        error = await NetworkNamespaces.AddAsync(
            new NetworkNamespace("mynetns", 51), invocationOptions);
        
        var newNamespaces = await NetworkNamespaces.GetAllAsync(invocationOptions);
        Console.WriteLine(newNamespaces);
    }
}