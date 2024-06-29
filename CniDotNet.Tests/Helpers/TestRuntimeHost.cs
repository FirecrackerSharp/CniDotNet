using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using FluentAssertions;

namespace CniDotNet.Tests.Helpers;

public class TestRuntimeHost : IRuntimeHost
{
    private int _invocationIndex;
    private readonly List<string> _acceptJsons = [];
    
    private readonly Dictionary<string, string?> _acceptEnvironment = new();
    private readonly List<string> _rejectEnvironment = [];
    private string? _returnedValue;
    
    public void AcceptEnvironment(string name, string value)
    {
        _acceptEnvironment[name] = value;
    }

    public void AcceptEnvironment(PluginOptions pluginOptions)
    {
        if (pluginOptions.ContainerId is null)
        {
            _rejectEnvironment.Add("CNI_CONTAINERID");
        }
        else
        {
            _acceptEnvironment["CNI_CONTAINERID"] = pluginOptions.ContainerId;
        }

        if (pluginOptions.NetworkNamespace is null)
        {
            _rejectEnvironment.Add("CNI_NETNS");
        }
        else
        {
            _acceptEnvironment["CNI_NETNS"] = pluginOptions.NetworkNamespace;
        }

        if (pluginOptions.InterfaceName is null)
        {
            _rejectEnvironment.Add("CNI_IFNAME");
        }
        else
        {
            _acceptEnvironment["CNI_IFNAME"] = pluginOptions.InterfaceName;
        }

        if (pluginOptions.IncludePath)
        {
            _acceptEnvironment["CNI_PATH"] = null;
        }
        else
        {
            _rejectEnvironment.Add("CNI_PATH");
        }
    }

    public void AcceptInput(Plugin plugin, PluginOptions pluginOptions, CniAddResult? addResult = null)
    {
        _acceptJsons.Add(DerivePluginInput(plugin, pluginOptions, addResult));
    }

    public void AcceptInput(PluginList pluginList, PluginOptions pluginOptions, CniAddResult addResult,
        bool skipFirst = false, bool backwards = false)   
    {
        var startIndex = skipFirst ? 1 : 0;
        if (backwards)
        {
            startIndex = pluginList.Plugins.Count - 1;
        }

        if (skipFirst)
        {
            _acceptJsons.Add(DerivePluginInput(pluginList.Plugins[0], pluginOptions, addResult: null));
        }

        for (var i = startIndex; i < pluginList.Plugins.Count; ++i)
        {
            var plugin = pluginList.Plugins[i];
            _acceptJsons.Add(DerivePluginInput(plugin, pluginOptions, addResult));
        }
    }

    public void Return<T>(T value)
    {
        _returnedValue = JsonSerializer.Serialize(value, CniRuntime.SerializerOptions);
    }

    public void ReturnNothing()
    {
        _returnedValue = string.Empty;
    }
    
    public Task WriteFileAsync(string path, string content, CancellationToken cancellationToken)
    {
        return Task.CompletedTask; // not implemented
    }

    public bool DirectoryExists(string path)
    {
        return true;
    }

    public Task<string> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty); // not implemented
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken)
    {
        return Task.CompletedTask; // not implemented
    }

    public Task<IEnumerable<string>> EnumerateDirectoryAsync(string path, string searchPattern, SearchOption searchOption,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<string>>([path + "/" + searchPattern]);
    }

    public Task<string?> GetEnvironmentVariableAsync(string variableName, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null); // not implemented
    }

    public Task<IRuntimeHostProcess> StartProcessAsync(string command, Dictionary<string, string> environment, InvocationOptions invocationOptions,
        CancellationToken cancellationToken)
    {
        foreach (var (key, value) in _acceptEnvironment)
        {
            environment.Should().ContainKey(key);
            
            if (value is not null)
            {
                environment[key].Should().Be(value);
            }
        }

        foreach (var notExpect in _rejectEnvironment)
        {
            environment.Should().NotContainKey(notExpect);
        }

        if (_acceptJsons.Count - _invocationIndex >= 1)
        {
            var acceptJson = _acceptJsons[_invocationIndex];
            var actualJson = command.Split("<<<").Last().TrimStart(' ').Trim('\'');
            actualJson.Should().Be(acceptJson);
        }

        _invocationIndex++;

        return Task.FromResult<IRuntimeHostProcess>(
            new TestRuntimeHostProcess(_returnedValue ?? throw new NullReferenceException()));
    }
    
    private static string DerivePluginInput(Plugin plugin, PluginOptions pluginOptions, CniAddResult? addResult)
    {
        var jsonNode = plugin.PluginParameters.DeepClone();
        
        jsonNode["cniVersion"] = pluginOptions.CniVersion;
        jsonNode["name"] = pluginOptions.Name;
        jsonNode["type"] = plugin.Type;

        if (plugin.Capabilities is not null)
        {
            jsonNode["runtimeConfig"] = plugin.Capabilities.DeepClone();
        }

        if (plugin.Args is not null)
        {
            jsonNode["args"] = plugin.Args.DeepClone();
        }

        var extraCapabilities = pluginOptions.ExtraCapabilities;
        if (extraCapabilities is not null)
        {
            jsonNode["runtimeConfig"] ??= new JsonObject();
            foreach (var (capabilityKey, capabilityValue) in extraCapabilities)
            {
                if (capabilityValue is null) continue;
                if (!jsonNode["runtimeConfig"]!.AsObject().ContainsKey(capabilityKey))
                {
                    jsonNode["runtimeConfig"]![capabilityKey] = capabilityValue.DeepClone();
                }
            }
        }

        if (addResult is not null)
        {
            jsonNode["prevResult"] = JsonSerializer
                .SerializeToNode(addResult, CniRuntime.SerializerOptions)!.AsObject();
        }

        return JsonSerializer.Serialize(jsonNode, CniRuntime.SerializerOptions);
    }
}