using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using CniDotNet.Runtime.Exceptions;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class RuntimeCheckTests
{
    [Theory, CustomAutoData]
    public async Task CheckPluginAsync_ShouldHandleSuccess(Plugin plugin, CniAddResult addResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "CHECK");
                host.AcceptInput(plugin, pluginOptions, addResult);
                host.ReturnNothing();
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.CheckPluginAsync(plugin, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
            });
    }

    [Theory, CustomAutoData]
    public async Task CheckPluginAsync_ShouldHandleError(Plugin plugin, CniAddResult addResult,
        CniErrorResult errorResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "CHECK");
                host.AcceptInput(plugin, pluginOptions, addResult);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.CheckPluginAsync(plugin, runtimeOptions, addResult);
                invocation.IsError.Should().BeTrue();
                invocation.IsSuccess.Should().BeFalse();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
            });
    }

    [Theory, CustomAutoData]
    public async Task CheckPluginListAsync_ShouldHandleSuccess(PluginList pluginList, CniAddResult addResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "CHECK");
                host.AcceptInput(pluginList, pluginOptions, addResult);
                host.ReturnNothing();
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.CheckPluginListAsync(pluginList, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
                invocation.ErrorCausePlugin.Should().BeNull();
            });
    }

    [Theory, CustomAutoData]
    public async Task CheckPluginListAsync_ShouldHandleError(PluginList pluginList, CniAddResult addResult, CniErrorResult errorResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "CHECK");
                host.AcceptInput(pluginList, pluginOptions, addResult);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.CheckPluginListAsync(pluginList, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeFalse();
                invocation.IsError.Should().BeTrue();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
                invocation.ErrorCausePlugin.Should().Be(pluginList.Plugins[0]);
            });
    }

    [Theory, CustomAutoData]
    public async Task CheckPluginListAsync_ShouldThrowForEmpty(PluginList pluginList, CniAddResult addResult)
    {
        pluginList = pluginList with { Plugins = [] };
        await FluentActions
            .Awaiting(
                async () => await CniRuntime.CheckPluginListAsync(pluginList, Exec.EmptyRuntimeOptions, addResult))
            .Should().ThrowAsync<CniEmptyPluginListException>();
    }

    [Theory, CustomAutoData]
    public async Task CheckPluginListWithStoredResultAsync_ShouldThrowForNotConfiguredStore(PluginList pluginList)
    {
        await FluentActions
            .Awaiting(async () => await CniRuntime
                .CheckPluginListWithStoredResultAsync(pluginList, Exec.EmptyRuntimeOptions))
            .Should().ThrowAsync<CniStoreRetrievalException>();
    }

    [Theory, CustomAutoData]
    public async Task CheckPluginListWithStoredResultAsync_ShouldThrowForNotResultStore(PluginList pluginList)
    {
        var runtimeOptions = Exec.EmptyRuntimeOptions with
        {
            InvocationStoreOptions = new InvocationStoreOptions(MemoryInvocationStore.Instance, StoreResults: false)
        };
        await FluentActions
            .Awaiting(async () => await CniRuntime
                .CheckPluginListWithStoredResultAsync(pluginList, runtimeOptions))
            .Should().ThrowAsync<CniStoreRetrievalException>();
    }

    [Theory, CustomAutoData]
    public async Task CheckPluginListWithStoredResultAsync_ShouldNotThrowForCorrectStore(PluginList pluginList, CniAddResult addResult)
    {
        await MemoryInvocationStore.Instance.SetResultAsync(pluginList, addResult, CancellationToken.None);
        var runtimeOptions = Exec.EmptyRuntimeOptions with
        {
            InvocationStoreOptions = new InvocationStoreOptions(MemoryInvocationStore.Instance, StoreResults: true)
        };
        await FluentActions
            .Awaiting(async () => await CniRuntime
                .CheckPluginListWithStoredResultAsync(pluginList, runtimeOptions))
            .Should().NotThrowAsync<CniStoreRetrievalException>();
    }
}