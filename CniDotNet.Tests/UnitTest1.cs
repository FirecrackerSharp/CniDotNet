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
        var ptp = new PtpNetwork(
            new HostLocalIpam(
                ResolvConf: "/etc/resolv.conf",
                Ranges:
                [
                    [new HostLocalIpamRange(Subnet: "192.168.127.0/24")]
                ]),
            IpMasq: true);
        var firewall = new FirewallNetwork();
        var tcRedirectTap = new TcRedirectTapNetwork();

        var typedNetworkList = new TypedNetworkList(
            CniVersion: new Version(1, 0, 0),
            Name: "fcnet",
            [ptp, firewall, tcRedirectTap]);
        var networkList = typedNetworkList.Build();
        
        var invocationOptions = new InvocationOptions(
            LocalCniHost.Current, "495762");
        var cniInvocationOptions = CniInvocationOptions.FromNetworkList(
            networkList,
            containerId: "fcnet",
            networkNamespace: "/var/run/netns/testing",
            invocationOptions,
            interfaceName: "eth0",
            pluginPath: "/home/kanpov/plugins/bin");

        var plo = new PluginLookupOptions("/home/kanpov/plugins/bin");
        var wrappedResult = await CniRuntime.AddNetworkListAsync(
            networkList, cniInvocationOptions, pluginLookupOptions: plo);
        var previousResult = wrappedResult.SuccessValue!;

        var checkErrorResult = await CniRuntime.CheckNetworkListAsync(
            networkList, cniInvocationOptions, previousResult, pluginLookupOptions: plo);
        Assert.Null(checkErrorResult);

        for (var i = 0; i < 1; ++i)
        {
            var deleteErrorResult = await CniRuntime.DeleteNetworkListAsync(
                networkList, cniInvocationOptions, previousResult,
                pluginLookupOptions: plo);
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