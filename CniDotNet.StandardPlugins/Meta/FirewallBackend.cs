using System.Runtime.Serialization;

namespace CniDotNet.StandardPlugins.Meta;

public enum FirewallBackend
{
    [EnumMember(Value = "iptables")]
    Iptables,
    [EnumMember(Value = "firewalld")]
    Firewalld
}