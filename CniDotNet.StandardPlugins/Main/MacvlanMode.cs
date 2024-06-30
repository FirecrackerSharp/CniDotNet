using System.Runtime.Serialization;

namespace CniDotNet.StandardPlugins.Main;

public enum MacvlanMode
{
    [EnumMember(Value = "bridge")]
    Bridge,
    [EnumMember(Value = "private")]
    Private,
    [EnumMember(Value = "vepa")]
    Vepa,
    [EnumMember(Value = "passthru")]
    Passthru
}