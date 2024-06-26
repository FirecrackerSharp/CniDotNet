using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Host.Local;
using CniDotNet.Runtime;
using FluentAssertions;

namespace CniDotNet.Tests;

public class PluginListsTests
{
    private const string ExpectJson = 
        """
        {
        	"cniVersion": "1.0.0",
        	"name": "plugin-list",
        	"plugins": [
        		{
        			"type": "my-type",
        			"capabilities": {
        				"a": "b"
        			},
        			"args": {
        				"c": "d"
        			},
        			"e": "f"
        		}
        	],
        	"cniVersions": ["0.1.0", "0.2.0", "0.3.0", "1.0.0"],
        	"disableCheck": true,
        	"disableGC": true
        }
        """;
    
    [Fact]
    public void LoadFromString_ShouldDeserialize()
    {
	    AssertPluginList(PluginLists.LoadFromString(ExpectJson));
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldRead()
    {
	    var filePath = $"/tmp/{Guid.NewGuid()}";
	    await File.WriteAllTextAsync(filePath, ExpectJson);
	    AssertPluginList(await PluginLists.LoadFromFileAsync(LocalRuntimeHost.Instance, filePath));
	    File.Delete(filePath);
    }

    [Fact]
    public async Task SearchAsync_ShouldReadEnvVar()
    {
	    Environment.SetEnvironmentVariable("CONF_LIST_PATH", "/tmp", EnvironmentVariableTarget.Process);
	    for (var i = 0; i < 3; ++i)
	    {
		    await File.WriteAllTextAsync($"/tmp/{i}.conflist", ExpectJson);
		    await File.WriteAllTextAsync($"/tmp/{i}.notconflist", ExpectJson);
	    }

	    var matches = await PluginLists.SearchAsync(LocalRuntimeHost.Instance,
		    new PluginListSearchOptions([".conflist"]));
	    matches.Count.Should().Be(3);
    }

    private static void AssertPluginList(PluginList actualPluginList)
    {
        actualPluginList.CniVersion.Should().Be("1.0.0");
        actualPluginList.Name.Should().Be("plugin-list");
        actualPluginList.Plugins.Count.Should().Be(1);
        actualPluginList.CniVersions.Should().BeEquivalentTo(["0.1.0", "0.2.0", "0.3.0", "1.0.0"]);
        actualPluginList.DisableCheck.Should().BeTrue();
        actualPluginList.DisableGc.Should().BeTrue();
        
        var actualPlugin = actualPluginList.Plugins[0];
        actualPlugin.PluginParameters.ToJsonString().Should().Be("""{"e":"f"}""");
        actualPlugin.Capabilities?.ToJsonString().Should().Be("""{"a":"b"}""");
        actualPlugin.Args?.ToJsonString().Should().Be("""{"c":"d"}""");
    }
}