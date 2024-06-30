using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record BridgePlugin(
    object? Ipam = null,
    string? BridgeName = null,
    bool? IsGateway = null,
    bool? IsDefaultGateway = null,
    bool? ForceAddress = null,
    bool? IpMasquerade = null,
    uint? Mtu = null,
    bool? HairpinMode = null,
    bool? PromiscuousMode = null,
    uint? VlanTag = null,
    bool? PreserveDefaultVlan = null,
    IReadOnlyList<BridgeVlanTrunk>? VlanTrunk = null,
    bool? EnableDuplicateAddressDetection = null,
    bool? MacSpoofCheck = null,
    bool? DisableContainerInterface = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("bridge", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (Ipam is not null)
        {
            jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);
        }

        if (BridgeName is not null)
        {
            jsonObject["bridge"] = BridgeName;
        }

        if (IsGateway is not null)
        {
            jsonObject["isGateway"] = IsGateway;
        }

        if (IsDefaultGateway is not null)
        {
            jsonObject["isDefaultGateway"] = IsDefaultGateway;
        }

        if (ForceAddress is not null)
        {
            jsonObject["forceAddress"] = ForceAddress;
        }

        if (IpMasquerade is not null)
        {
            jsonObject["ipMasq"] = IpMasquerade;
        }

        if (Mtu is not null)
        {
            jsonObject["mtu"] = Mtu;
        }

        if (HairpinMode is not null)
        {
            jsonObject["hairpinMode"] = HairpinMode;
        }

        if (PromiscuousMode is not null)
        {
            jsonObject["promiscMode"] = PromiscuousMode;
        }

        if (VlanTag is not null)
        {
            jsonObject["vlan"] = VlanTag;
        }

        if (PreserveDefaultVlan is not null)
        {
            jsonObject["preserveDefaultVlan"] = PreserveDefaultVlan;
        }

        if (VlanTrunk is not null)
        {
            jsonObject["vlanTrunk"] = JsonSerializer.SerializeToNode(VlanTrunk, CniRuntime.SerializerOptions);
        }

        if (EnableDuplicateAddressDetection is not null)
        {
            // this is cursed
            jsonObject["enabledad"] = EnableDuplicateAddressDetection;
        }

        if (MacSpoofCheck is not null)
        {
            jsonObject["macspoofchk"] = MacSpoofCheck;
        }

        if (DisableContainerInterface is not null)
        {
            jsonObject["disableContainerInterface"] = DisableContainerInterface;
        }
    }
}