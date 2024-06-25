using System.Text.Json.Nodes;

namespace CniDotNet.Typing.Main;

public sealed record HostDevicePlugin(
    string? DeviceName = null,
    string? DeviceMac = null,
    string? DeviceKernelObject = null,
    string? DevicePciBusId = null,
    JsonObject? Args = null,
    JsonObject? Capabilities = null) : TypedPlugin("host-device", Args, Capabilities)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
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