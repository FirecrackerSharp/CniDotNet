using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;
using FluentAssertions;

namespace CniDotNet.Tests.Helpers;

public class JsonContract<T>(T instance)
{
    private readonly Dictionary<string, object?> _directMatchers = new();
    private readonly List<object?> _mergeMatchers = [];

    public JsonContract<T> Contains(string key, Func<T, object?> propertySelector)
    {
        _directMatchers[key] = propertySelector(instance);
        return this;
    }

    public JsonContract<T> MergesWith(Func<T, object?> propertySelector)
    {
        _mergeMatchers.Add(propertySelector(instance));
        return this;
    }

    public void Test()
    {
        Test(i => JsonSerializer.SerializeToNode(i, CniRuntime.SerializerOptions)!.AsObject());
    }

    public void Test(Func<T, JsonObject> serializer)
    {
        var jsonObject = serializer(instance);
        
        foreach (var (key, value) in _directMatchers)
        {
            jsonObject.AsObject().Should().ContainKey(key);

            var expectedValue = JsonSerializer.Serialize(value, CniRuntime.SerializerOptions);
            var actualValue = jsonObject[key]!.ToJsonString(CniRuntime.SerializerOptions);
            expectedValue.Should().Be(actualValue);
        }

        foreach (var mergedValue in _mergeMatchers)
        {
            var value = JsonSerializer.SerializeToNode(mergedValue, CniRuntime.SerializerOptions)!.AsObject();

            foreach (var (innerKey, innerValue) in value)
            {
                jsonObject.Should().ContainKey(innerKey);
                var expectedValue = JsonSerializer.Serialize(innerValue, CniRuntime.SerializerOptions);
                var actualValue = jsonObject[innerKey]!.ToJsonString(CniRuntime.SerializerOptions);
                expectedValue.Should().Be(actualValue);
            }
        }
    }
}