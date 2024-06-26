using System.Text.Json.Nodes;
using CniDotNet.Abstractions;
using CniDotNet.Host.Ssh;
using CniDotNet.Host.Local;
using CniDotNet.Data.Options;
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
        var portMap = new PortMapPlugin(Capabilities: new TypedCapabilities(
            PortMappings: [new TypedCapabilityPortMapping(1000, 2000, TypedCapabilityPortProtocol.Udp)]));

        var typedPluginList = new TypedPluginList(
            CniVersion: new Version(1, 0, 0),
            Name: "fcnet",
            [ptp, firewall, tcRedirectTap, portMap]);
        var pluginList = typedPluginList.Build();
        var serial = PluginLists.SaveToString(pluginList);
        
        var runtimeOptions = new RuntimeOptions(
            PluginOptions.FromPluginList(pluginList, "fcnet", "/var/run/netns/testing", "eth0",
                extraCapabilities: new JsonObject { ["q"] = "a" }), 
            new InvocationOptions(LocalRuntimeHost.Instance, "495762"),
            new PluginSearchOptions(Directory: "/usr/libexec/cni"),
            new InvocationStoreOptions(InMemoryInvocationStore.Instance));
        
        var wrappedResult = await CniRuntime.AddPluginListAsync(pluginList, runtimeOptions);

        await CniRuntime.GarbageCollectPluginListWithStoreAsync(pluginList, runtimeOptions);
        var errorResult = await CniRuntime.DeletePluginListWithStoreAsync(pluginList, runtimeOptions);
        Assert.Null(errorResult);
    }

    [Fact]
    public async Task Test2()
    {
        using var cniHost = new SshRuntimeHost(new PasswordConnectionInfo("172.20.2.11", 8009, "root", "495762"));
        var invocationOptions = new InvocationOptions(cniHost);
        
        var namespaces = await NetworkNamespaces.GetAllAsync(invocationOptions);
        Console.WriteLine(namespaces);
    }
}