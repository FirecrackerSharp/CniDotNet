using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;
using CniDotNet.Typing;
using FluentAssertions;

namespace CniDotNet.StandardPlugins.Tests;

public class JsonContract<T>(T instance)
{
    private readonly Dictionary<string, object?> _directMatchers = new();

    public JsonContract<T> Contains(string key, Func<T, object?> propertySelector)
    {
        _directMatchers[key] = propertySelector(instance);
        return this;
    }

    public void Test()
    {
        Test(i => JsonSerializer.SerializeToNode(i, CniRuntime.SerializerOptions)!.AsObject());
    }

    public void TestPlugin()
    {
        Test(i =>
        {
            if (i is not TypedPlugin plugin) throw new ArgumentException();
            var jsonObject = new JsonObject();
            plugin.SerializePluginParameters(jsonObject);
            return jsonObject;
        });
    }

    private void Test(Func<T, JsonObject> serializer)
    {
        var jsonObject = serializer(instance);
        
        foreach (var (key, value) in _directMatchers)
        {
            jsonObject.Should().ContainKey(key);

            var expectedValue = JsonSerializer.Serialize(value, CniRuntime.SerializerOptions);
            var actualValue = jsonObject[key]!.ToJsonString(CniRuntime.SerializerOptions);
            expectedValue.Should().Be(actualValue);
        }
    }
}