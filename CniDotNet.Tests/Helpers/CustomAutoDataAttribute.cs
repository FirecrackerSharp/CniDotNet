using System.Text.Json.Nodes;
using AutoFixture;
using AutoFixture.Xunit2;
using CniDotNet.Data;
using CniDotNet.Typing;

namespace CniDotNet.Tests.Helpers;

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

    fixture.Customize<TypedCapabilities>(c => c
        .With(p => p.ExtraCapabilities, () =>
            new JsonObject { ["cap1"] = "cap2", ["cap3"] = "cap4" }));

    fixture.Customize<TypedArgs>(c => c
        .With(a => a.ExtraArgs, () =>
            new JsonObject { ["arg1"] = "arg2", ["arg3"] = "arg4" }));
    
    return fixture;
});