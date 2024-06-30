using System.Text.Json;
using System.Text.Json.Nodes;
using AutoFixture;
using CniDotNet.Runtime;
using CniDotNet.Typing;
using FluentAssertions;

namespace CniDotNet.StandardPlugins.Tests;

public class JsonContract<T>
{
    private static Fixture Fixture
    {
        get
        {
            var fixture = new Fixture();
            
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(fixture);

            fixture.Customize<TypedCapabilities>(c => c
                .With(p => p.ExtraCapabilities, () =>
                    new JsonObject { ["cap1"] = "cap2", ["cap3"] = "cap4" }));

            fixture.Customize<TypedArgs>(c => c
                .With(a => a.ExtraArgs, () =>
                    new JsonObject { ["arg1"] = "arg2", ["arg3"] = "arg4" }));

            return fixture;
        }
    }
    
    private readonly Dictionary<string, object?> _directMatchers = new();
    private readonly T _instance = Fixture.Create<T>();

    public JsonContract<T> Contains(string key, Func<T, object?> propertySelector)
    {
        _directMatchers[key] = propertySelector(_instance);
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
        var jsonObject = serializer(_instance);
        
        foreach (var (key, value) in _directMatchers)
        {
            jsonObject.Should().ContainKey(key);

            var expectedValue = JsonSerializer.Serialize(value, CniRuntime.SerializerOptions);
            var actualValue = jsonObject[key]!.ToJsonString(CniRuntime.SerializerOptions);
            expectedValue.Should().Be(actualValue);
        }
    }
}