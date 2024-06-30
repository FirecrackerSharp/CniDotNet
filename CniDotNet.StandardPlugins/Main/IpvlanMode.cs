using System.Runtime.Serialization;

namespace CniDotNet.StandardPlugins.Main;

public enum IpvlanMode
{
    [EnumMember(Value = "l2")]
    L2,
    [EnumMember(Value = "l3")]
    L3,
    [EnumMember(Value = "l3s")]
    L3S
}