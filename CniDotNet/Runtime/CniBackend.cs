using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;
using CniDotNet.Runtime.Exceptions;

namespace CniDotNet.Runtime;

internal static partial class CniBackend
{
    private static readonly Regex CniRegex = CniRegexGenerator();
    private const int MaximumInterfaceNameLength = 15;
    
    internal static void ValidatePluginOptions(PluginOptions pluginOptions, string operation,
        PluginOptionRequirement requirements)
    {
        if (pluginOptions.SkipValidation) return;
        
        // path
        if (requirements.HasFlag(PluginOptionRequirement.Path) && !pluginOptions.IncludePath)
        {
            throw new CniValidationFailureException(
                $"Path is required for \"{operation}\" but is excluded according to IncludePath=false");
        }
        
        // network namespace
        if (requirements.HasFlag(PluginOptionRequirement.NetworkNamespace) && string.IsNullOrWhiteSpace(pluginOptions.NetworkNamespace))
        {
            throw new CniValidationFailureException(
                $"Network namespace is required for \"{operation}\" but isn't provided");
        }
        
        // network name
        if (string.IsNullOrWhiteSpace(pluginOptions.Name))
        {
            throw new CniValidationFailureException("Network name is required for any operation but is missing");
        }

        if (!CniRegex.IsMatch(pluginOptions.Name))
        {
            throw new CniValidationFailureException($"Network name \"{pluginOptions.Name}\" doesn't match regex");
        }
        
        // container ID
        if (requirements.HasFlag(PluginOptionRequirement.ContainerId) && string.IsNullOrWhiteSpace(pluginOptions.ContainerId))
        {
            throw new CniValidationFailureException($"Container ID is required for \"{operation}\" but isn't provided");
        }
        
        if (pluginOptions.ContainerId is not null && !CniRegex.IsMatch(pluginOptions.ContainerId))
        {
            throw new CniValidationFailureException($"Container ID \"{pluginOptions.ContainerId}\" doesn't match regex");
        }
        
        // interface name
        if (requirements.HasFlag(PluginOptionRequirement.InterfaceName) && string.IsNullOrWhiteSpace(pluginOptions.InterfaceName))
        {
            throw new CniValidationFailureException($"Interface name is required for \"{operation}\" but isn't provided");
        }

        if (pluginOptions.InterfaceName is null) return;

        if (pluginOptions.InterfaceName.Length > MaximumInterfaceNameLength)
        {
            throw new CniValidationFailureException(
                $"Interface name \"{pluginOptions.InterfaceName}\" is longer than the maximum of {MaximumInterfaceNameLength}");
        }

        if (pluginOptions.InterfaceName is "." or "..")
        {
            throw new CniValidationFailureException("Interface name is either . or .., neither of which are allowed");
        }

        if (pluginOptions.InterfaceName.Any(c => c is '/' or ':' || char.IsWhiteSpace(c)))
        {
            throw new CniValidationFailureException(
                $"Interface name \"{pluginOptions.InterfaceName}\" contains a forbidden character (/, : or a space)");
        }
    }

    internal static async Task<string> SearchForPluginBinaryAsync(Plugin plugin, RuntimeOptions runtimeOptions,
        CancellationToken cancellationToken)
    {
        var usesCache = runtimeOptions.InvocationStoreOptions is { StoreBinaryLocations: true };
        if (usesCache)
        {
            var hitLocation = await runtimeOptions.InvocationStoreOptions!.InvocationStore
                .GetBinaryLocationAsync(plugin.Type, cancellationToken);
            if (hitLocation is not null)
            {
                runtimeOptions.PluginSearchOptions.CachedActualDirectory = hitLocation;
                return hitLocation;
            }
        }
        
        var matchFromTable = runtimeOptions.PluginSearchOptions.SearchTable?.GetValueOrDefault(plugin.Type);
        if (matchFromTable is not null) return matchFromTable;

        var directory = await runtimeOptions.PluginSearchOptions.GetActualDirectoryAsync(
                runtimeOptions.InvocationOptions.RuntimeHost, cancellationToken);
        if (directory is null)
        {
            throw new CniBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: directory wasn't specified and " +
                                              $"environment variable doesn't exist");
        }

        if (!runtimeOptions.InvocationOptions.RuntimeHost.DirectoryExists(directory))
        {
            throw new CniBinaryNotFoundException($"Could not find \"{plugin.Type}\" plugin: \"{directory}\" directory " +
                                              $"doesn't exist");
        }

        var matchingFiles = await runtimeOptions.InvocationOptions.RuntimeHost.EnumerateDirectoryAsync(
            directory, plugin.Type, runtimeOptions.PluginSearchOptions.DirectorySearchOption,
            cancellationToken);
        var missLocation = matchingFiles.FirstOrDefault();
        if (missLocation is null)
        {
            throw new CniBinaryNotFoundException(
                $"Could not find \"{plugin.Type}\" plugin: the file doesn't exist according to the given search option" +
                $"in the \"{directory}\" directory");
        }

        if (usesCache)
        {
            await runtimeOptions.InvocationStoreOptions!.InvocationStore.SetBinaryLocationAsync(
                plugin.Type, missLocation, cancellationToken);
        }

        return missLocation;
    }

    internal static async Task<string> InvokeAsync(
        Plugin plugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        CniAddResult? addResult,
        IEnumerable<Attachment>? validAttachments,
        CancellationToken cancellationToken)
    {
        var stdinJson = DerivePluginInput(plugin, runtimeOptions, addResult, validAttachments);
        
        var environment = new Dictionary<string, string> { { Constants.Environment.Command, operation } };
        if (runtimeOptions.PluginOptions.ContainerId is not null)
        {
            environment[Constants.Environment.ContainerId] = runtimeOptions.PluginOptions.ContainerId;
        }
        if (runtimeOptions.PluginOptions.InterfaceName is not null)
        {
            environment[Constants.Environment.InterfaceName] = runtimeOptions.PluginOptions.InterfaceName;
        }
        if (runtimeOptions.PluginOptions.NetworkNamespace is not null)
        {
            environment[Constants.Environment.NetworkNamespace] = runtimeOptions.PluginOptions.NetworkNamespace;
        }
        if (runtimeOptions.PluginSearchOptions.CachedActualDirectory is not null && runtimeOptions.PluginOptions.IncludePath)
        {
            environment[Constants.Environment.PluginPath] = runtimeOptions.PluginSearchOptions.CachedActualDirectory;
        }

        var process = await runtimeOptions.InvocationOptions.RuntimeHost.StartProcessAsync(
            $"{pluginBinary} <<< '{stdinJson}'", environment, runtimeOptions.InvocationOptions, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return process.CurrentOutput;
    }

    private static string DerivePluginInput(Plugin plugin, RuntimeOptions runtimeOptions, CniAddResult? previousResult,
        IEnumerable<Attachment>? validAttachments)
    {
        var jsonNode = plugin.PluginParameters.DeepClone();
        
        jsonNode[Constants.Parsing.CniVersion] = runtimeOptions.PluginOptions.CniVersion;
        jsonNode[Constants.Parsing.Name] = runtimeOptions.PluginOptions.Name;
        jsonNode[Constants.Parsing.Type] = plugin.Type;

        if (plugin.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] = plugin.Capabilities.DeepClone();
        }

        if (plugin.Args is not null)
        {
            jsonNode[Constants.Parsing.Args] = plugin.Args.DeepClone();
        }

        var extraCapabilities = runtimeOptions.PluginOptions.ExtraCapabilities;
        if (extraCapabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] ??= new JsonObject();
            foreach (var (capabilityKey, capabilityValue) in extraCapabilities)
            {
                if (capabilityValue is null) continue;
                if (!jsonNode[Constants.Parsing.RuntimeConfig]!.AsObject().ContainsKey(capabilityKey))
                {
                    jsonNode[Constants.Parsing.RuntimeConfig]![capabilityKey] = capabilityValue.DeepClone();
                }
            }
        }

        if (previousResult is not null)
        {
            jsonNode[Constants.Parsing.PreviousResult] = JsonSerializer
                .SerializeToNode(previousResult, CniRuntime.SerializerOptions)!.AsObject();
        }

        if (validAttachments is not null)
        {
            var jsonArray = new JsonArray();

            foreach (var gcAttachment in validAttachments)
            {
                jsonArray.Add(new JsonObject
                {
                    [Constants.Parsing.GcContainerId] = gcAttachment.PluginOptions.ContainerId!,
                    [Constants.Parsing.GcInterfaceName] = gcAttachment.PluginOptions.InterfaceName!
                });
            }

            jsonNode[Constants.Parsing.GcAttachments] = jsonArray;
        }

        return JsonSerializer.Serialize(jsonNode, CniRuntime.SerializerOptions);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9_.\-]*$")]
    private static partial Regex CniRegexGenerator();
}