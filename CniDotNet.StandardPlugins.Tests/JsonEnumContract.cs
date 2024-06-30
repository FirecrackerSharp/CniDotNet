using System.Text.Json;
using CniDotNet.Runtime;
using FluentAssertions;

namespace CniDotNet.StandardPlugins.Tests;

public class JsonEnumContract<T>
{
    public JsonEnumContract<T> For(T value, string serializedValue)
    {
        var actualValue = JsonSerializer.Serialize(value, CniRuntime.SerializerOptions);
        actualValue.Should().Be($"\"{serializedValue}\"");
        return this;
    }
}