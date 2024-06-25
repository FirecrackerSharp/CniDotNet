using System.Runtime.Serialization;

namespace CniDotNet.Typing;

public enum TypedCapabilityPortProtocol
{
    [EnumMember(Value = "tcp")]
    Tcp,
    [EnumMember(Value = "udp")]
    Udp
}