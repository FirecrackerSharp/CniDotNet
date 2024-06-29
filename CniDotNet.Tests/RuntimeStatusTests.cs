using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Runtime;
using CniDotNet.Runtime.Exceptions;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class RuntimeStatusTests
{
    [Theory, CustomAutoData]
    public async Task StatusPluginAsync_ShouldHandleSuccess(Plugin plugin)
    {
        await SuccessPluginAsync(plugin, h => h.ReturnNothing());
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginAsync_ShouldSwallowUnsupportedError(Plugin plugin, CniErrorResult errorResult)
    {
        errorResult = errorResult with { Code = 4 };
        await SuccessPluginAsync(plugin, h => h.Return(errorResult));
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginAsync_ShouldHandleError(Plugin plugin, CniErrorResult errorResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "STATUS");
                host.AcceptInput(plugin, pluginOptions);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.StatusPluginAsync(plugin, runtimeOptions);
                invocation.IsSuccess.Should().BeFalse();
                invocation.IsError.Should().BeTrue();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
            });
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginAsync_ShouldValidate(Plugin plugin)
    {
        await Exec.ValidationTestAsync(CniRuntime.VersionRequirements, r => CniRuntime.StatusPluginAsync(plugin, r));
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginListAsync_ShouldHandleSuccess(PluginList pluginList)
    {
        await SuccessPluginListAsync(pluginList, h => h.ReturnNothing());
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginListAsync_ShouldSwallowUnsupportedError(PluginList pluginList,
        CniErrorResult errorResult)
    {
        errorResult = errorResult with { Code = 4 };
        await SuccessPluginListAsync(pluginList, h => h.Return(errorResult));
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginListAsync_ShouldHandleError(PluginList pluginList, CniErrorResult errorResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "STATUS");
                host.AcceptInput(pluginList, pluginOptions);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.StatusPluginListAsync(pluginList, runtimeOptions);
                invocation.IsSuccess.Should().BeFalse();
                invocation.IsError.Should().BeTrue();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
                invocation.ErrorCausePlugin.Should().Be(pluginList.Plugins[0]);
            });
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginListAsync_ShouldValidate(PluginList pluginList)
    {
        await Exec.ValidationTestAsync(CniRuntime.VersionRequirements,
            r => CniRuntime.StatusPluginListAsync(pluginList, r));
    }

    [Theory, CustomAutoData]
    public async Task StatusPluginListAsync_ShouldThrowForEmpty(PluginList pluginList)
    {
        pluginList = pluginList with { Plugins = [] };
        await FluentActions
            .Awaiting(async () => await CniRuntime.StatusPluginListAsync(pluginList, Exec.EmptyRuntimeOptions))
            .Should().ThrowAsync<CniEmptyPluginListException>();
    }

    private static async Task SuccessPluginAsync(Plugin plugin, Action<TestRuntimeHost> configure)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "STATUS");
                host.AcceptInput(plugin, pluginOptions);
                configure(host);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.StatusPluginAsync(plugin, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
            });
    }

    private static async Task SuccessPluginListAsync(PluginList pluginList, Action<TestRuntimeHost> configure)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "STATUS");
                host.AcceptInput(pluginList, pluginOptions);
                configure(host);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.StatusPluginListAsync(pluginList, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
                invocation.ErrorCausePlugin.Should().BeNull();
            });
    }
}