using CniDotNet.StandardPlugins.Meta;

namespace CniDotNet.StandardPlugins.Tests;

public class MetaTests
{
    [Theory, CustomAutoData]
    public void BandwidthPlugin(BandwidthPlugin instance)
    {
        new JsonContract<BandwidthPlugin>(instance)
            .Contains("ingressRate", x => x.IngressRate)
            .Contains("ingressBurst", x => x.IngressBurst)
            .Contains("egressRate", x => x.EgressRate)
            .Contains("egressBurst", x => x.EgressBurst)
            .TestPlugin();
    }
}