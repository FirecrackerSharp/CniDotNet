using System.Text.Json.Nodes;
using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using CniDotNet.Ssh;
using CniDotNet.StandardPlugins.Ipam;
using CniDotNet.StandardPlugins.Main;
using CniDotNet.StandardPlugins.Meta;
using CniDotNet.Typing;
using Renci.SshNet;

namespace CniDotNet.Tests;

public class UnitTest
{
    [Fact]
    public async Task Verify()
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
            [ptp, firewall, tcRedirectTap]);
        var pluginList = typedPluginList.Build();

        using var host = new SshRuntimeHost(new PasswordConnectionInfo("172.20.2.11", 8009, "root", "495762"));
        
        var runtimeOptions = new RuntimeOptions(
            PluginOptions.FromPluginList(pluginList, "fcnet", "/var/run/netns/testing", "eth0",
                extraCapabilities: new JsonObject { ["q"] = "a" }), 
            new InvocationOptions(host, "495762"),
            new PluginSearchOptions(Directory: "/root/plugins"),
            new InvocationStoreOptions(InMemoryInvocationStore.Instance));

        await NetworkNamespaces.AddAsync(new NetworkNamespace("testing", 5), runtimeOptions.InvocationOptions);
        var i = await CniRuntime.AddPluginListAsync(pluginList, runtimeOptions);
        var k = await CniRuntime.GarbageCollectWithStoreAsync([pluginList], runtimeOptions);
        
        var j = await CniRuntime.DeletePluginListWithStoredResultAsync(pluginList, runtimeOptions);
    }
}