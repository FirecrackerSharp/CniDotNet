using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using FluentAssertions;

namespace CniDotNet.Tests;

public class PluginListsTests
{
    private const string ValidPluginListJson = 
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
    private const string ValidPluginListJsonWithOmissions =
	    """
	    {
	    	"cniVersion": "1.0.0",
	    	"name": "plugin-list",
	    	"plugins": [
	    		{
	    			"type": "my-type",
	    			"e": "f"
	    		}
	    	]
	    }
	    """;
    
    [Fact]
    public void LoadFromString_ShouldDeserializeWithoutOmissions()
    {
	    AssertPluginList(PluginLists.LoadFromString(ValidPluginListJson));
    }

    [Fact]
    public void LoadFromString_ShouldDeserializeWithOmissions()
    {
	    AssertPluginListWithOmissions(PluginLists.LoadFromString(ValidPluginListJsonWithOmissions));
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReadWithoutOmissions()
    {
	    var filePath = $"/tmp/{Guid.NewGuid()}";
	    await File.WriteAllTextAsync(filePath, ValidPluginListJson);
	    AssertPluginList(await PluginLists.LoadFromFileAsync(LocalRuntimeHost.Instance, filePath));
	    File.Delete(filePath);
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReadWithOmissions()
    {
	    var filePath = $"/tmp/{Guid.NewGuid()}";
	    await File.WriteAllTextAsync(filePath, ValidPluginListJsonWithOmissions);
	    AssertPluginListWithOmissions(await PluginLists.LoadFromFileAsync(LocalRuntimeHost.Instance, filePath));
	    File.Delete(filePath);
    }

    [Fact]
    public async Task SearchAsync_ShouldReadEnvVar()
    {
	    Environment.SetEnvironmentVariable("CONF_LIST_PATH", "/tmp/a", EnvironmentVariableTarget.Process);
	    await ArrangeSearchAsync(
		    new PluginListSearchOptions([".conflist"]),
		    matches => matches.Count.Should().Be(3));
	    await ArrangeSearchAsync(
		    new PluginListSearchOptions([".conflist"], DirectorySearchOption: SearchOption.AllDirectories),
		    matches => matches.Count.Should().Be(6));
	    Environment.SetEnvironmentVariable("CONF_LIST_PATH", "", EnvironmentVariableTarget.Process);
    }

    [Fact]
    public async Task SearchAsync_ShouldReadDirectory()
    {
	    await ArrangeSearchAsync(
		    new PluginListSearchOptions([".conflist"], Directory: "/tmp/a"),
		    matches => matches.Count.Should().Be(3));
	    await ArrangeSearchAsync(
		    new PluginListSearchOptions([".conflist"], Directory: "/tmp/a", DirectorySearchOption: SearchOption.AllDirectories),
		    matches => matches.Count.Should().Be(6));
    }

    [Fact]
    public async Task SearchAsync_ShouldHandleFailures()
    {
	    await ArrangeSearchAsync(
		    new PluginListSearchOptions([".conflist"], Directory: "/tmp/a", ProceedAfterFailure: false),
		    matches => matches.Should().BeEmpty(),
		    corrupt: true);
	    await ArrangeSearchAsync(
		    new PluginListSearchOptions([".conflist"], Directory: "/tmp/a"),
		    matches => matches.Count.Should().Be(2) /* 3-1=5 (1 skipped) */,
		    corrupt: true);
    }

    private static async Task ArrangeSearchAsync(PluginListSearchOptions pluginListSearchOptions,
	    Action<IReadOnlyList<PluginList>> matchAssertion, bool corrupt = false)
    {
	    Directory.CreateDirectory("/tmp/a");
		Directory.CreateDirectory("/tmp/a/b");

	    for (var i = 0; i < 3; ++i)
	    {
		    await File.WriteAllTextAsync($"/tmp/a/{i}.conflist", ValidPluginListJson);
		    await File.WriteAllTextAsync($"/tmp/a/b/{i}.conflist", ValidPluginListJson);
		    await File.WriteAllTextAsync($"/tmp/a/{i}.notconflist", ValidPluginListJson);
		    await File.WriteAllTextAsync($"/tmp/a/b/{i}.notconflist", ValidPluginListJson);
	    }

	    if (corrupt)
	    {
		    await File.WriteAllTextAsync("/tmp/a/1.conflist", "_" + ValidPluginListJson);
	    }

	    var matches = await PluginLists.SearchAsync(LocalRuntimeHost.Instance, pluginListSearchOptions);
	    matchAssertion(matches);
	    
	    Directory.Delete("/tmp/a", recursive: true);
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
    
    private static void AssertPluginListWithOmissions(PluginList actualPluginList)
    {
	    actualPluginList.CniVersion.Should().Be("1.0.0");
	    actualPluginList.Name.Should().Be("plugin-list");
	    actualPluginList.Plugins.Count.Should().Be(1);
	    actualPluginList.CniVersions.Should().BeNull();
	    actualPluginList.DisableCheck.Should().BeFalse();
	    actualPluginList.DisableGc.Should().BeFalse();
	    
	    actualPluginList.Plugins[0].PluginParameters.ToJsonString().Should().Be("""{"e":"f"}""");
    }
}