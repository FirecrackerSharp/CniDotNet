using CniDotNet.Tests.Helpers;
using CniDotNet.Typing;

namespace CniDotNet.Tests;

public class TypingTests
{
    [Theory, CustomAutoData]
    public void TypedCapabilityDns(TypedCapabilityDns instance)
    {
        new JsonContract<TypedCapabilityDns>(instance)
            .Contains("searches", x => x.Searches)
            .Contains("servers", x => x.Servers)
            .Test();
    }

    [Theory, CustomAutoData]
    public void TypedCapabilityBandwidth(TypedCapabilityBandwidth instance)
    {
        new JsonContract<TypedCapabilityBandwidth>(instance)
            .Contains("ingressRate", x => x.IngressRate)
            .Contains("ingressBurst", x => x.IngressBurst)
            .Contains("egressRate", x => x.EgressRate)
            .Contains("egressBurst", x => x.EgressBurst)
            .Test();
    }

    [Theory, CustomAutoData]
    public void TypedCapabilityIpRange(TypedCapabilityIpRange instance)
    {
        new JsonContract<TypedCapabilityIpRange>(instance)
            .Contains("subnet", x => x.Subnet)
            .Contains("rangeStart", x => x.RangeStart)
            .Contains("rangeEnd", x => x.RangeEnd)
            .Contains("gateway", x => x.Gateway)
            .Test();
    }

    [Theory, CustomAutoData]
    public void TypedCapabilityPortMapping(TypedCapabilityPortMapping instance)
    {
        new JsonContract<TypedCapabilityPortMapping>(instance)
            .Contains("hostPort", x => x.HostPort)
            .Contains("containerPort", x => x.ContainerPort)
            .Contains("protocol", x => x.Protocol)
            .Test();
    }

    [Theory, CustomAutoData]
    public void TypedCapabilities(TypedCapabilities instance)
    {
        new JsonContract<TypedCapabilities>(instance)
            .Contains("portMappings", x => x.PortMappings)
            .Contains("ipRanges", x => x.IpRanges)
            .Contains("bandwidth", x => x.Bandwidth)
            .Contains("dns", x => x.Dns)
            .Contains("ips", x => x.Ips)
            .Contains("mac", x => x.Mac)
            .Contains("infinibandGUID", x => x.InfinibandGuid)
            .Contains("deviceID", x => x.DeviceId)
            .Contains("aliases", x => x.Aliases)
            .Contains("cgroupPath", x => x.CgroupPath)
            .MergesWith(x => x.ExtraCapabilities)
            .Test(x => x.Serialize());
    }

    [Theory, CustomAutoData]
    public void TypedArgLabel(TypedArgLabel instance)
    {
        new JsonContract<TypedArgLabel>(instance)
            .Contains("key", x => x.Key)
            .Contains("value", x => x.Value)
            .Test();
    }

    [Theory, CustomAutoData]
    public void TypedArgs(TypedArgs instance)
    {
        new JsonContract<TypedArgs>(instance)
            .Contains("labels", x => x.Labels)
            .Contains("ips", x => x.Ips)
            .MergesWith(x => x.ExtraArgs)
            .Test(x => x.Serialize());
    }
}