using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;

namespace CniDotNet.Typing;

public sealed record TypedCapabilities(
    IReadOnlyList<TypedCapabilityPortMapping>? PortMappings = null,
    TypedCapabilityIpRange[][]? IpRanges = null,
    TypedCapabilityBandwidth? Bandwidth = null,
    TypedCapabilityDns? Dns = null,
    IReadOnlyList<string>? Ips = null,
    string? Mac = null,
    string? InfinibandGuid = null,
    string? DeviceId = null,
    IReadOnlyList<string>? Aliases = null,
    string? CgroupPath = null,
    JsonObject? ExtraCapabilities = null)
{
    public JsonObject Serialize()
    {
        var jsonNode = ExtraCapabilities?.DeepClone() ?? new JsonObject();

        if (PortMappings is not null)
        {
            jsonNode["portMappings"] = JsonSerializer.SerializeToNode(PortMappings, CniRuntime.SerializerOptions);
        }

        if (IpRanges is not null)
        {
            jsonNode["ipRanges"] = JsonSerializer.SerializeToNode(IpRanges, CniRuntime.SerializerOptions);
        }

        if (Bandwidth is not null)
        {
            jsonNode["bandwidth"] = JsonSerializer.SerializeToNode(Bandwidth, CniRuntime.SerializerOptions);
        }

        if (Dns is not null)
        {
            jsonNode["dns"] = JsonSerializer.SerializeToNode(Dns, CniRuntime.SerializerOptions);
        }

        if (Ips is not null)
        {
            jsonNode["ips"] = JsonSerializer.SerializeToNode(Ips, CniRuntime.SerializerOptions);
        }

        if (Mac is not null)
        {
            jsonNode["mac"] = Mac;
        }

        if (InfinibandGuid is not null)
        {
            jsonNode["infinibandGUID"] = InfinibandGuid;
        }

        if (DeviceId is not null)
        {
            jsonNode["deviceID"] = DeviceId;
        }

        if (Aliases is not null)
        {
            jsonNode["aliases"] = JsonSerializer.SerializeToNode(Aliases, CniRuntime.SerializerOptions);
        }

        if (CgroupPath is not null)
        {
            jsonNode["cgroupPath"] = CgroupPath;
        }

        return jsonNode.AsObject();
    }
}