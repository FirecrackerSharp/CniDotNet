using CniDotNet.StandardPlugins.Main;

namespace CniDotNet.StandardPlugins.Tests;

public class MainPluginTests
{
    [Fact]
    public void BridgePlugin()
    {
        new JsonContract<BridgePlugin>()
            .Contains("ipam", x => x.Ipam)
            .Contains("bridge", x => x.BridgeName)
            .Contains("isGateway", x => x.IsGateway)
            .Contains("isDefaultGateway", x => x.IsDefaultGateway)
            .Contains("forceAddress", x => x.ForceAddress)
            .Contains("ipMasq", x => x.IpMasquerade)
            .Contains("mtu", x => x.Mtu)
            .Contains("hairpinMode", x => x.HairpinMode)
            .Contains("promiscMode", x => x.PromiscuousMode)
            .Contains("vlan", x => x.VlanTag)
            .Contains("preserveDefaultVlan", x => x.PreserveDefaultVlan)
            .Contains("vlanTrunk", x => x.VlanTrunk)
            .Contains("enabledad", x => x.EnableDuplicateAddressDetection)
            .Contains("macspoofchk", x => x.MacSpoofCheck)
            .Contains("disableContainerInterface", x => x.DisableContainerInterface)
            .TestPlugin();
    }

    [Fact]
    public void BridgeVlanTrunk()
    {
        new JsonContract<BridgeVlanTrunk>()
            .Contains("minID", x => x.MinId)
            .Contains("maxID", x => x.MaxId)
            .Contains("ID", x => x.Id)
            .TestNonPlugin();
    }

    [Fact]
    public void DummyPlugin()
    {
        new JsonContract<DummyPlugin>()
            .Contains("ipam", x => x.Ipam)
            .TestPlugin();
    }

    [Fact]
    public void HostDevicePlugin()
    {
        new JsonContract<HostDevicePlugin>()
            .Contains("device", x => x.DeviceName)
            .Contains("hwaddr", x => x.DeviceMac)
            .Contains("kernelpath", x => x.DeviceKernelObject)
            .Contains("pciBusID", x => x.DevicePciBusId)
            .TestPlugin();
    }

    [Fact]
    public void IpvlanModeEnum()
    {
        new JsonEnumContract<IpvlanMode>()
            .For(IpvlanMode.L2, "l2")
            .For(IpvlanMode.L3, "l3")
            .For(IpvlanMode.L3S, "l3s");
    }

    [Fact]
    public void IpvlanPlugin()
    {
        new JsonContract<IpvlanPlugin>()
            .Contains("ipam", x => x.Ipam)
            .Contains("master", x => x.Master)
            .Contains("mode", x => x.Mode)
            .Contains("mtu", x => x.Mtu)
            .Contains("linkInContainer", x => x.LinkInContainer)
            .TestPlugin();
    }

    [Fact]
    public void MacvlanModeEnum()
    {
        new JsonEnumContract<MacvlanMode>()
            .For(MacvlanMode.Bridge, "bridge")
            .For(MacvlanMode.Passthru, "passthru")
            .For(MacvlanMode.Private, "private")
            .For(MacvlanMode.Vepa, "vepa");
    }

    [Fact]
    public void MacvlanPlugin()
    {
        new JsonContract<MacvlanPlugin>()
            .Contains("ipam", x => x.Ipam)
            .Contains("master", x => x.Master)
            .Contains("mode", x => x.Mode)
            .Contains("mtu", x => x.Mtu)
            .Contains("linkInContainer", x => x.LinkInContainer)
            .Contains("mac", x => x.Mac)
            .TestPlugin();
    }

    [Fact]
    public void PtpPlugin()
    {
        new JsonContract<PtpPlugin>()
            .Contains("ipMasq", x => x.IpMasquerade)
            .Contains("mtu", x => x.Mtu)
            .Contains("ipam", x => x.Ipam)
            .Contains("dns", x => x.Dns)
            .TestPlugin();
    }

    [Fact]
    public void TapPlugin()
    {
        new JsonContract<TapPlugin>()
            .Contains("mac", x => x.Mac)
            .Contains("mtu", x => x.Mtu)
            .Contains("selinuxcontext", x => x.SeLinuxContext)
            .Contains("multiQueue", x => x.MultiQueue)
            .Contains("owner", x => x.OwnerUid)
            .Contains("group", x => x.GroupGid)
            .Contains("bridge", x => x.Bridge)
            .TestPlugin();
    }

    [Fact]
    public void TcRedirectTapPlugin()
    {
        new JsonContract<TcRedirectTapPlugin>()
            .TestPlugin();
    }

    [Fact]
    public void VlanPlugin()
    {
        new JsonContract<VlanPlugin>()
            .Contains("master", x => x.Master)
            .Contains("vlanId", x => x.VlanId)
            .Contains("ipam", x => x.Ipam)
            .Contains("mtu", x => x.Mtu)
            .Contains("dns", x => x.Dns)
            .Contains("linkInContainer", x => x.LinkInContainer)
            .TestPlugin();
    }
}