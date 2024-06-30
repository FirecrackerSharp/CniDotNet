using CniDotNet.StandardPlugins.Ipam;

namespace CniDotNet.StandardPlugins.Tests;

public class IpamTests
{
    [Fact]
    public void DhcpLeaseOptions()
    {
        new JsonContract<DhcpLeaseOptions>()
            .Contains("option", x => x.Option)
            .Contains("value", x => x.Value)
            .Contains("fromArg", x => x.FromArgument)
            .TestNonPlugin();
    }

    [Fact]
    public void DhcpRequestOptions()
    {
        new JsonContract<DhcpRequestOptions>()
            .Contains("skipDefault", x => x.SkipDefault)
            .Contains("option", x => x.Option)
            .TestNonPlugin();
    }

    [Fact]
    public void DhcpIpam()
    {
        new JsonContract<DhcpIpam>()
            .Contains("daemonSocketPath", x => x.DaemonSocketPath)
            .Contains("request", x => x.RequestOptions)
            .Contains("provide", x => x.LeaseOptions)
            .TestNonPlugin();
    }

    [Fact]
    public void GenericIpamRoute()
    {
        new JsonContract<GenericIpamRoute>()
            .Contains("dst", x => x.Destination)
            .Contains("gw", x => x.Gateway)
            .TestNonPlugin();
    }

    [Fact]
    public void HostLocalIpam()
    {
        new JsonContract<HostLocalIpam>()
            .Contains("ranges", x => x.Ranges)
            .Contains("type", x => x.Type)
            .Contains("resolvConf", x => x.ResolvConf)
            .Contains("dataDir", x => x.DataDir)
            .Contains("routes", x => x.Routes)
            .TestNonPlugin();
    }

    [Fact]
    public void StaticIpam()
    {
        new JsonContract<StaticIpam>()
            .Contains("type", x => x.Type)
            .Contains("addresses", x => x.Addresses)
            .Contains("routes", x => x.Routes)
            .Contains("dns", x => x.Dns)
            .TestNonPlugin();
    }

    [Fact]
    public void StaticIpamAddress()
    {
        new JsonContract<StaticIpamAddress>()
            .Contains("address", x => x.Address)
            .Contains("gateway", x => x.Gateway)
            .TestNonPlugin();
    }
}