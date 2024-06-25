using CniDotNet.CniHost.Ssh;
using CniDotNet.Host.Local;
using CniDotNet.Data;
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
                Ranges: [[new HostLocalIpamRange(Subnet: "192.168.127.0/24")]]),
            IpMasq: true);
        var firewall = new FirewallPlugin();
        var tcRedirectTap = new TcRedirectTapPlugin();

        var typedPluginList = new TypedPluginList(
            CniVersion: new Version(1, 0, 0),
            Name: "fcnet",
            [ptp, firewall, tcRedirectTap]);
        var pluginList = typedPluginList.Build();

        using var cniHost = new SshCniHost(new PasswordConnectionInfo("172.20.2.11", 8009, "root", "495762"));
        
        var cniRuntimeOptions = new RuntimeOptions(
            PluginOptions.FromPluginList(pluginList, "fcnet", "/var/run/netns/testing", "eth0"), 
            new InvocationOptions(cniHost, "495762"),
            new PluginSearchOptions(Directory: "/root/plugins"));
        
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