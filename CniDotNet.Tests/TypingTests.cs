using System.Text.Json.Nodes;
using CniDotNet.Data;
using CniDotNet.Runtime;
using CniDotNet.Tests.Helpers;
using CniDotNet.Typing;
using FluentAssertions;

namespace CniDotNet.Tests;

public class TypingTests
{
    private static EventHandler<bool>? _serializedEvent;
    
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

    [Theory, CustomAutoData]
    public void TypedPlugin(MockTypedPlugin typedPlugin)
    {
        var invoked = false;
        _serializedEvent += (_, _) => invoked = true;
        
        var builtPlugin = typedPlugin.Build();
        VerifyPluginBuild(typedPlugin, builtPlugin);
        invoked.Should().BeTrue();
    }

    [Theory, CustomAutoData]
    public void TypedPluginList(List<MockTypedPlugin> plugins,
        Version cniVersion, string name, List<Version> cniVersions, bool disableCheck, bool disableGc)
    {
        var typedList = new TypedPluginList(cniVersion, name, plugins, cniVersions, disableCheck, disableGc);
        var invocationCount = 0;
        _serializedEvent += (_, _) => invocationCount++;
        
        var builtList = typedList.Build();
        invocationCount.Should().Be(plugins.Count);
        builtList.Name.Should().Be(name);
        builtList.CniVersion.Should().Be(cniVersion.ToString());
        builtList.CniVersions.Should().BeEquivalentTo(cniVersions.Select(x => x.ToString()));
        builtList.DisableCheck.Should().Be(disableCheck);
        builtList.DisableGc.Should().Be(disableGc);

        for (var i = 0; i < plugins.Count; ++i)
        {
            VerifyPluginBuild(plugins[i], builtList.Plugins[i]);
        }
    }

    private static void VerifyPluginBuild(TypedPlugin typedPlugin, Plugin builtPlugin)
    {
        builtPlugin.Type.Should().Be(typedPlugin.Type);
        ShouldEquivalentlySerialize(builtPlugin.Args!, typedPlugin.Args!.Serialize());
        ShouldEquivalentlySerialize(builtPlugin.Capabilities!, typedPlugin.Capabilities!.Serialize());
    }

    private static void ShouldEquivalentlySerialize(JsonObject first, JsonObject second)
    {
        var firstJson = first.ToJsonString(CniRuntime.SerializerOptions);
        var secondJson = second.ToJsonString(CniRuntime.SerializerOptions);
        firstJson.Should().Be(secondJson);
    }

    public record MockTypedPlugin(TypedCapabilities Capabilities, TypedArgs Args)
        : TypedPlugin("mock", Capabilities, Args)
    {
        public override void SerializePluginParameters(JsonObject jsonObject)
        {
            _serializedEvent?.Invoke(this, true);
        }
    }
}