using System.Text.Json.Nodes;
using AutoFixture;
using AutoFixture.Xunit2;
using CniDotNet.Data;

namespace CniDotNet.Tests;

public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
{
    var fixture = new Fixture();
    
    // workaround so that JsonNode's and JsonObject's can be generated
    var customization = new SupportMutableValueTypesCustomization();
    customization.Customize(fixture);
    
    fixture.Customize<Plugin>(c => c
        .With(p => p.PluginParameters, () => new JsonObject { ["param"] = "value" })
        .With(p => p.Args, () => new JsonObject { ["arg"] = "value" })
        .With(p => p.Capabilities, () => new JsonObject { ["capability"] = "value" }));
    return fixture;
});