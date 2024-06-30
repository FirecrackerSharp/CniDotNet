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

    [Fact]
    public void FirewallPlugin()
    {
        new JsonContract<FirewallPlugin>()
            .Contains("backend", x => x.Backend)
            .Contains("iptablesAdminChainName", x => x.IptablesAdminChainName)
            .Contains("firewalldZone", x => x.FirewalldZone)
            .Contains("ingressPolicy", x => x.IngressPolicy)
            .TestPlugin();
    }

    [Fact]
    public void PortMapPlugin()
    {
        new JsonContract<PortMapPlugin>()
            .Contains("snat", x => x.Snat)
            .Contains("masqAll", x => x.MasqueradeAll)
            .Contains("markMasqBit", x => x.MarkMasqueradeBit)
            .Contains("externalSetMarkChain", x => x.ExternalSetMarkChain)
            .Contains("conditionsV4", x => x.ConditionsIpV4)
            .Contains("conditionsV6", x => x.ConditionsIpV6)
            .TestPlugin();
    }

    [Fact]
    public void TuningPlugin()
    {
        new JsonContract<TuningPlugin>()
            .Contains("dataDir", x => x.DataDir)
            .Contains("mac", x => x.Mac)
            .Contains("mtu", x => x.Mtu)
            .Contains("txQLen", x => x.TxQLen)
            .Contains("promisc", x => x.PromiscuousMode)
            .Contains("allmulti", x => x.AllMulticastMode)
            .Contains("sysctl", x => x.Sysctl)
            .TestPlugin();
    }

    [Fact]
    public void VrfPlugin()
    {
        new JsonContract<VrfPlugin>()
            .Contains("vrfname", x => x.VrfName)
            .Contains("table", x => x.RouteTable)
            .TestPlugin();
    }
}