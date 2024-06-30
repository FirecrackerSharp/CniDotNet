using CniDotNet.StandardPlugins.Meta;

namespace CniDotNet.StandardPlugins.Tests;

public class MetaTests
{
    [Fact]
    public void BandwidthPlugin()
    {
        new JsonContract<BandwidthPlugin>()
            .Contains("ingressRate", x => x.IngressRate)
            .Contains("ingressBurst", x => x.IngressBurst)
            .Contains("egressRate", x => x.EgressRate)
            .Contains("egressBurst", x => x.EgressBurst)
            .TestPlugin();
    }
}