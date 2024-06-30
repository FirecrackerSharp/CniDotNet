using System.Text.Json.Nodes;
using AutoFixture;
using AutoFixture.Xunit2;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Tests;

public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
{
    var fixture = new Fixture();
    
    // workaround so that JsonNode's and JsonObject's can be generated
    var customization = new SupportMutableValueTypesCustomization();
    customization.Customize(fixture);

    fixture.Customize<TypedCapabilities>(c => c
        .With(p => p.ExtraCapabilities, () =>
            new JsonObject { ["cap1"] = "cap2", ["cap3"] = "cap4" }));

    fixture.Customize<TypedArgs>(c => c
        .With(a => a.ExtraArgs, () =>
            new JsonObject { ["arg1"] = "arg2", ["arg3"] = "arg4" }));
    
    return fixture;
});