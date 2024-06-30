using System.Text.Json.Nodes;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record HostDevicePlugin(
    string? DeviceName = null,
    string? DeviceMac = null,
    string? DeviceKernelObject = null,
    string? DevicePciBusId = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("host-device", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (DeviceName is not null)
        {
            jsonObject["device"] = DeviceName;
        }

        if (DeviceMac is not null)
        {
            jsonObject["hwaddr"] = DeviceMac;
        }

        if (DeviceKernelObject is not null)
        {
            jsonObject["kernelpath"] = DeviceKernelObject;
        }

        if (DevicePciBusId is not null)
        {
            jsonObject["pciBusID"] = DevicePciBusId;
        }
    }
}