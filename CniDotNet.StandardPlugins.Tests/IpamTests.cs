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
            .Test();
    }

    [Fact]
    public void DhcpRequestOptions()
    {
        new JsonContract<DhcpRequestOptions>()
            .Contains("skipDefault", x => x.SkipDefault)
            .Contains("option", x => x.Option)
            .Test();
    }

    [Fact]
    public void DhcpIpam()
    {
        new JsonContract<DhcpIpam>()
            .Contains("daemonSocketPath", x => x.DaemonSocketPath)
            .Contains("request", x => x.RequestOptions)
            .Contains("provide", x => x.LeaseOptions)
            .Test();
    }

    [Fact]
    public void GenericIpamRoute()
    {
        new JsonContract<GenericIpamRoute>()
            .Contains("dst", x => x.Destination)
            .Contains("gw", x => x.Gateway)
            .Test();
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
            .Test();
    }

    [Fact]
    public void StaticIpam()
    {
        new JsonContract<StaticIpam>()
            .Contains("type", x => x.Type)
            .Contains("addresses", x => x.Addresses)
            .Contains("routes", x => x.Routes)
            .Contains("dns", x => x.Dns)
            .Test();
    }

    [Fact]
    public void StaticIpamAddress()
    {
        new JsonContract<StaticIpamAddress>()
            .Contains("address", x => x.Address)
            .Contains("gateway", x => x.Gateway)
            .Test();
    }
}