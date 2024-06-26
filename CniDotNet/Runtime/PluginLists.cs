using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.Options;

namespace CniDotNet.Runtime;

public static class PluginLists
{
    public static async Task<PluginList?> SearchFirstAsync(
        IRuntimeHost runtimeHost, PluginListSearchOptions pluginListSearchOptions, CancellationToken cancellationToken = default)
    {
        var matches = await SearchAsync(runtimeHost, pluginListSearchOptions, cancellationToken);
        return matches.Count == 0 ? null : matches[0];
    }
    
    public static async Task<IReadOnlyList<PluginList>> SearchAsync(
        IRuntimeHost runtimeHost, PluginListSearchOptions pluginListSearchOptions, CancellationToken cancellationToken = default)
    {
        var directory = pluginListSearchOptions.Directory ?? await runtimeHost.GetEnvironmentVariableAsync(
            pluginListSearchOptions.EnvironmentVariable, cancellationToken);
        if (directory is null) return [];

        if (!runtimeHost.DirectoryExists(directory)) return [];

        var files = await runtimeHost
            .EnumerateDirectoryAsync(directory, "",
                pluginListSearchOptions.DirectorySearchOption, cancellationToken);

        var pluginLists = new List<PluginList>();

        foreach (var file in files
                     .Where(f => pluginListSearchOptions.FileExtensions.Contains(Path.GetExtension(f))))
        {
            try
            {
                var pluginList = await LoadFromFileAsync(runtimeHost, file, cancellationToken);
                pluginLists.Add(pluginList);
            }
            catch (Exception)
            {
                if (pluginListSearchOptions.ProceedAfterFailure) continue;
                return [];
            }
        }

        return pluginLists;
    }
    
    public static async Task<PluginList> LoadFromFileAsync(
        IRuntimeHost runtimeHost, string filePath, CancellationToken cancellationToken = default)
    {
        var sourceString = await runtimeHost.ReadFileAsync(filePath, cancellationToken);
        return LoadFromString(sourceString);
    }

    public static PluginList LoadFromString(string sourceString)
    {
        var jsonNode = JsonSerializer.Deserialize<JsonNode>(sourceString)!;
        var configuration = LoadPluginList(jsonNode);

        return configuration;
    }

    public static async Task SaveToFileAsync(
        PluginList pluginList, IRuntimeHost runtimeHost, string filePath, bool prettyPrint = true, CancellationToken cancellationToken = default)
    {
        var content = SaveToString(pluginList, prettyPrint);
        await runtimeHost.WriteFileAsync(filePath, content, cancellationToken);
    }

    public static string SaveToString(PluginList pluginList, bool prettyPrint = true)
    {
        var jsonObject = SavePluginList(pluginList);
        return JsonSerializer.Serialize(jsonObject,
            prettyPrint ? CniRuntime.PrettyPrintSerializerOptions : CniRuntime.SerializerOptions);
    }

    private static PluginList LoadPluginList(JsonNode jsonNode)
    {
        var cniVersion = jsonNode[Constants.Parsing.CniVersion]!.GetValue<string>();

        IEnumerable<string>? cniVersions = null;
        if (jsonNode.AsObject().ContainsKey(Constants.Parsing.CniVersions))
        {
            cniVersions = jsonNode[Constants.Parsing.CniVersions]!.AsArray().GetValues<string>();
        }

        var name = jsonNode[Constants.Parsing.Name]!.GetValue<string>();
        var disableCheck = false;

        if (jsonNode.AsObject().ContainsKey(Constants.Parsing.DisableCheck))
        {
            disableCheck = jsonNode[Constants.Parsing.DisableCheck]!.GetValue<bool>();
        }

        var disableGc = false;
        if (jsonNode.AsObject().ContainsKey(Constants.Parsing.DisableGc))
        {
            disableGc = jsonNode[Constants.Parsing.DisableGc]!.GetValue<bool>();
        }

        var networks = jsonNode[Constants.Parsing.Plugins]!.AsArray()
            .Select(pluginJsonNode => LoadPlugin(pluginJsonNode!))
            .ToList();

        return new PluginList(
            cniVersion, name, networks, cniVersions, disableCheck, disableGc);
    }

    private static Plugin LoadPlugin(JsonNode jsonNode)
    {
        var type = jsonNode[Constants.Parsing.Type]!.GetValue<string>();
        var capabilities = jsonNode[Constants.Parsing.Capabilities]?.AsObject();
        var args = jsonNode[Constants.Parsing.Args]?.AsObject();

        var pluginParameters = jsonNode.AsObject();
        pluginParameters.Remove(Constants.Parsing.Type);
        if (capabilities is not null) pluginParameters.Remove(Constants.Parsing.Capabilities);
        if (args is not null) pluginParameters.Remove(Constants.Parsing.Args);

        return new Plugin(type, capabilities, args, pluginParameters);
    }

    private static JsonObject SavePluginList(PluginList pluginList)
    {
        var jsonObject = new JsonObject
        {
            [Constants.Parsing.CniVersion] = pluginList.CniVersion,
            [Constants.Parsing.Name] = pluginList.Name
        };

        if (pluginList.CniVersions is not null)
        {
            var versionArray = new JsonArray();
            foreach (var cniVersion in pluginList.CniVersions)
            {
                versionArray.Add(cniVersion);
            }

            jsonObject[Constants.Parsing.CniVersions] = versionArray;
        }

        if (pluginList.DisableCheck)
        {
            jsonObject[Constants.Parsing.DisableCheck] = pluginList.DisableCheck;
        }

        if (pluginList.DisableGc)
        {
            jsonObject[Constants.Parsing.DisableGc] = pluginList.DisableGc;
        }

        var networkArray = new JsonArray();
        foreach (var network in pluginList.Plugins)
        {
            networkArray.Add(SavePlugin(network));
        }

        jsonObject[Constants.Parsing.Plugins] = networkArray;

        return jsonObject;
    }

    private static JsonNode SavePlugin(Plugin plugin)
    {
        var jsonNode = plugin.PluginParameters.DeepClone();
        jsonNode[Constants.Parsing.Type] = plugin.Type;

        if (plugin.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.Capabilities] = plugin.Capabilities.DeepClone();
        }

        if (plugin.Args is not null)
        {
            jsonNode[Constants.Parsing.Args] = plugin.Args.DeepClone();
        }

        return jsonNode;
    }
}