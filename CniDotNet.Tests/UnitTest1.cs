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
        var conf = await NetworkLists.LookupFirstAsync(LocalCniHost.Current,
            new NetworkListLookupOptions([".conflist"], Directory: "/etc/cni/net.d"));
        var invocationOptions = new InvocationOptions(
            LocalCniHost.Current, "495762");
        var cniInvocationOptions = CniInvocationOptions.FromNetworkList(
            conf!,
            containerId: "fcnet",
            networkNamespace: "/var/run/netns/testing",
            invocationOptions,
            interfaceName: "eth0",
            pluginPath: "/home/kanpov/plugins/bin");

        var plo = new PluginLookupOptions("/home/kanpov/plugins/bin");
        var wrappedResult = await CniRuntime.AddNetworkListAsync(
            conf!, cniInvocationOptions, pluginLookupOptions: plo);
        var previousResult = wrappedResult.SuccessValue!;

        var checkErrorResult = await CniRuntime.CheckNetworkListAsync(
            conf!, cniInvocationOptions, previousResult, pluginLookupOptions: plo);
        Assert.Null(checkErrorResult);

        var gcResult = await CniRuntime.GarbageCollectNetworkListAsync(
            conf!, cniInvocationOptions, pluginLookupOptions: plo);
        
        for (var i = 0; i < 2; ++i)
        {
            var deleteErrorResult = await CniRuntime.DeleteNetworkListAsync(
                conf!, cniInvocationOptions, previousResult,
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

    [Fact]
    public void Test3()
    {
        var ptpTypedNetwork = new PtpTypedNetwork(
            new HostLocalIpam(
                ResolvConf: "/etc/resolv.conf",
                Ranges:
                [
                    new HostLocalIpamRange(Subnet: "192.168.127.0/24")
                ]),
            IpMasq: true);

        var ptpNetwork = ptpTypedNetwork.Build();
        Console.WriteLine(ptpNetwork);
    }
}