using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Runtime;
using CniDotNet.Runtime.Exceptions;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class RuntimeVersionTests
{
    [Theory, CustomAutoData]
    public async Task VersionPluginAsync_ShouldHandleSuccess(Plugin plugin, CniVersionResult versionResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "VERSION");
                host.AcceptInput(plugin, pluginOptions);
                host.Return(versionResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.VersionPluginAsync(plugin, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.SuccessVersionResult.Should().BeEquivalentTo(versionResult);
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
            });
    }

    [Theory, CustomAutoData]
    public async Task VersionPluginAsync_ShouldHandleError(Plugin plugin, CniErrorResult errorResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "VERSION");
                host.AcceptInput(plugin, pluginOptions);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.VersionPluginAsync(plugin, runtimeOptions);
                invocation.IsSuccess.Should().BeFalse();
                invocation.SuccessVersionResult.Should().BeNull();
                invocation.IsError.Should().BeTrue();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
            });
    }

    [Theory, CustomAutoData]
    public async Task VersionPluginAsync_ShouldValidate(Plugin plugin)
    {
        await Exec.ValidationTestAsync(CniRuntime.VersionRequirements, r => CniRuntime.VersionPluginAsync(plugin, r));
    }

    [Theory, CustomAutoData]
    public async Task VersionPluginListAsync_ShouldHandleSuccess(PluginList pluginList, CniVersionResult versionResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "VERSION");
                host.AcceptInput(pluginList, pluginOptions);
                host.Return(versionResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.VersionPluginListAsync(pluginList, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
                invocation.ErrorCausePlugin.Should().BeNull();

                var results = new Dictionary<Plugin, CniVersionResult>();
                foreach (var plugin in pluginList.Plugins)
                {
                    results[plugin] = versionResult;
                }

                invocation.SuccessVersionResults.Should().BeEquivalentTo(results);
            });
    }

    [Theory, CustomAutoData]
    public async Task VersionPluginListAsync_ShouldHandleError(PluginList pluginList, CniErrorResult errorResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "VERSION");
                host.AcceptInput(pluginList, pluginOptions);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.VersionPluginListAsync(pluginList, runtimeOptions);
                invocation.IsSuccess.Should().BeFalse();
                invocation.SuccessVersionResults.Should().BeNull();
                invocation.IsError.Should().BeTrue();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
                invocation.ErrorCausePlugin.Should().Be(pluginList.Plugins[0]);
            });
    }

    [Theory, CustomAutoData]
    public async Task VersionPluginListAsync_ShouldThrowForEmptyList(PluginList pluginList)
    {
        pluginList = pluginList with { Plugins = [] };
        await FluentActions
            .Awaiting(async () => await CniRuntime.VersionPluginListAsync(pluginList, Exec.EmptyRuntimeOptions))
            .Should().ThrowAsync<CniEmptyPluginListException>();
    }

    [Theory, CustomAutoData]
    public async Task VersionPluginListAsync_ShouldValidate(PluginList pluginList)
    {
        await Exec.ValidationTestAsync(CniRuntime.VersionRequirements,
            r => CniRuntime.VersionPluginListAsync(pluginList, r));
    }
}