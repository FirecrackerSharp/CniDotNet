using System.Text.Json.Nodes;
using CniDotNet.Abstractions;
using CniDotNet.Data.Options;
using CniDotNet.Host.Local;
using CniDotNet.Runtime;
using CniDotNet.StandardPlugins.Ipam;
using CniDotNet.StandardPlugins.Main;
using CniDotNet.StandardPlugins.Meta;
using CniDotNet.Typing;

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
            [ptp, firewall, tcRedirectTap, portMap]);
        var pluginList = typedPluginList.Build();
        
        var runtimeOptions = new RuntimeOptions(
            PluginOptions.FromPluginList(pluginList, "fcnet", "/var/run/netns/testing", "eth0",
                extraCapabilities: new JsonObject { ["q"] = "a" }), 
            new InvocationOptions(LocalRuntimeHost.Instance, "495762"),
            new PluginSearchOptions(Directory: "/usr/libexec/cni"),
            new InvocationStoreOptions(InMemoryInvocationStore.Instance));

        var w = await CniRuntime.AddPluginListAsync(pluginList, runtimeOptions);
        var q = await CniRuntime.DeletePluginListWithStoreAsync(pluginList, runtimeOptions);
    }
}