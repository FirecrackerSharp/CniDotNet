using AutoFixture.Xunit2;
using CniDotNet.StandardPlugins.Ipam;

namespace CniDotNet.StandardPlugins.Tests;

public class IpamTests
{
    [Theory, AutoData]
    public void DhcpLeaseOptions(DhcpLeaseOptions instance)
    {
        new JsonContract<DhcpLeaseOptions>(instance)
            .Contains("option", x => x.Option)
            .Contains("value", x => x.Value)
            .Contains("fromArg", x => x.FromArgument)
            .Test();
    }

    [Theory, AutoData]
    public void DhcpRequestOptions(DhcpRequestOptions instance)
    {
        new JsonContract<DhcpRequestOptions>(instance)
            .Contains("skipDefault", x => x.SkipDefault)
            .Contains("option", x => x.Option)
            .Test();
    }

    [Theory, AutoData]
    public void DhcpIpam(DhcpIpam instance)
    {
        new JsonContract<DhcpIpam>(instance)
            .Contains("daemonSocketPath", x => x.DaemonSocketPath)
            .Contains("request", x => x.RequestOptions)
            .Contains("provide", x => x.LeaseOptions)
            .Test();
    }
}