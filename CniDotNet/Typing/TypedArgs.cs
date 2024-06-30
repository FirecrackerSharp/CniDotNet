using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;

namespace CniDotNet.Typing;

public sealed record TypedArgs(
    IReadOnlyList<TypedArgLabel>? Labels = null,
    IReadOnlyList<string>? Ips = null,
    JsonObject? ExtraArgs = null)
{
    public JsonObject Serialize()
    {
        var jsonNode = ExtraArgs?.DeepClone() ?? new JsonObject();

        if (Labels is not null)
        {
            jsonNode["labels"] = JsonSerializer.SerializeToNode(Labels, CniRuntime.SerializerOptions);
        }

        if (Ips is not null)
        {
            jsonNode["ips"] = JsonSerializer.SerializeToNode(Ips, CniRuntime.SerializerOptions);
        }

        return jsonNode.AsObject();
    }
}