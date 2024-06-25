using System.Text.Json;
using CniDotNet.Host.Ssh;
using CniDotNet.Host.Local;
using CniDotNet.Data;
using CniDotNet.StandardPlugins.Ipam;
using CniDotNet.StandardPlugins.Main;
using CniDotNet.StandardPlugins.Meta;
using CniDotNet.Runtime;
using CniDotNet.Typing;
using Renci.SshNet;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var ptp = new PtpPlugin(
            new HostLocalIpam(
                ResolvConf: "/etc/resolv.conf",
                Ranges: [[new TypedCapabilityIpRange(Subnet: "192.168.127.0/24")]]),
            IpMasquerade: true,
            Capabilities: new TypedCapabilities(
                PortMappings: [new TypedCapabilityPortMapping(1000, 2000)]),
            Args: new TypedArgs([new TypedArgLabel("key", "value")]));
        var firewall = new FirewallPlugin();
        var tcRedirectTap = new TcRedirectTapPlugin();
        var bandwidth = new BandwidthPlugin(123, 456, 123, 456);

        var typedPluginList = new TypedPluginList(
            CniVersion: new Version(1, 0, 0),
            Name: "fcnet",
            [ptp, firewall, tcRedirectTap, bandwidth]);
        var pluginList = typedPluginList.Build();
        var serial = PluginLists.SaveToString(pluginList);
        
        var cniRuntimeOptions = new RuntimeOptions(
            PluginOptions.FromPluginList(pluginList, "fcnet", "/var/run/netns/testing", "eth0"), 
            new InvocationOptions(LocalCniHost.Instance, "495762"),
            new PluginSearchOptions(Directory: "/usr/libexec/cni"));
        
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
        using var cniHost = new SshCniHost(new PasswordConnectionInfo("172.20.2.11", 8009, "root", "495762"));
        var invocationOptions = new InvocationOptions(cniHost);
        
        var namespaces = await NetworkNamespaces.GetAllAsync(invocationOptions);
        Console.WriteLine(namespaces);
    }
}