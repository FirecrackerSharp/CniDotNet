using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using CniDotNet.Runtime.Exceptions;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class RuntimeDeleteTests
{
    [Theory, CustomAutoData]
    public async Task DeletePluginAsync_ShouldHandleSuccess(CniAddResult addResult, Plugin plugin)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "DEL");
                host.AcceptInput(plugin, pluginOptions, addResult);
                host.ReturnNothing();
            },
            async runtimeOptions =>
            {
                await MemoryInvocationStore.Instance.AddAttachmentAsync(new Attachment(runtimeOptions.PluginOptions,
                    plugin, null), CancellationToken.None);
                
                var invocation = await CniRuntime.DeletePluginAsync(plugin, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();

                var storedAttachment = await MemoryInvocationStore.Instance.GetAttachmentAsync(plugin,
                    runtimeOptions.PluginOptions, CancellationToken.None);
                storedAttachment.Should().BeNull();
            });
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginAsync_ShouldHandleError(CniAddResult addResult, CniErrorResult errorResult,
        Plugin plugin)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "DEL");
                host.AcceptInput(plugin, pluginOptions, addResult);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                await MemoryInvocationStore.Instance.AddAttachmentAsync(new Attachment(runtimeOptions.PluginOptions,
                    plugin, null), CancellationToken.None);
                
                var invocation = await CniRuntime.DeletePluginAsync(plugin, runtimeOptions, addResult);
                invocation.IsError.Should().BeTrue();
                invocation.IsSuccess.Should().BeFalse();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);

                var storedAttachment = await MemoryInvocationStore.Instance.GetAttachmentAsync(plugin,
                    runtimeOptions.PluginOptions, CancellationToken.None);
                storedAttachment.Should().NotBeNull();
            });
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginAsync_ShouldHandleDisabledStore(CniAddResult addResult, Plugin plugin)
    {
        await Exec.RuntimeTestAsync(
            (host, _) => host.ReturnNothing(),
            async runtimeOptions =>
            {
                await MemoryInvocationStore.Instance.AddAttachmentAsync(new Attachment(runtimeOptions.PluginOptions,
                    plugin, null), CancellationToken.None);
                
                var invocation = await CniRuntime.DeletePluginAsync(plugin, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeTrue();
                
                var storedAttachment = await MemoryInvocationStore.Instance.GetAttachmentAsync(plugin,
                    runtimeOptions.PluginOptions, CancellationToken.None);
                storedAttachment.Should().NotBeNull();
            },
            disableInvocationStore: true);
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginAsync_ShouldValidate(Plugin plugin, CniAddResult addResult)
    {
        await Exec.ValidationTestAsync(CniRuntime.DeleteRequirements,
            r => CniRuntime.DeletePluginAsync(plugin, r, addResult));
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginListAsync_ShouldHandleSuccess(PluginList pluginList, CniAddResult addResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "DEL");
                host.AcceptInput(pluginList, pluginOptions, addResult, backwards: true);
                host.ReturnNothing();
            },
            async runtimeOptions =>
            {
                await MemoryInvocationStore.Instance.SetResultAsync(pluginList, addResult, CancellationToken.None);
                await MemoryInvocationStore.Instance.AddAttachmentAsync(
                    new Attachment(runtimeOptions.PluginOptions, pluginList.Plugins[0], pluginList),
                    CancellationToken.None);
                
                var invocation = await CniRuntime.DeletePluginListAsync(pluginList, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
                invocation.ErrorCausePlugin.Should().BeNull();

                var storedResult =
                    await MemoryInvocationStore.Instance.GetResultAsync(pluginList, CancellationToken.None);
                storedResult.Should().BeNull();
                var storedAttachments = await MemoryInvocationStore.Instance
                    .GetAllAttachmentsForPluginListAsync(pluginList, CancellationToken.None);
                storedAttachments.Should().BeEmpty();
            });
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginListAsync_ShouldHandleError(PluginList pluginList, CniAddResult addResult,
        CniErrorResult errorResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "DEL");
                host.AcceptInput(pluginList, pluginOptions, addResult, backwards: true);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.DeletePluginListAsync(pluginList, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeFalse();
                invocation.IsError.Should().BeTrue();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
                invocation.ErrorCausePlugin.Should().Be(pluginList.Plugins[^1]);
            });
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginListAsync_ShouldHandleDisabledStore(PluginList pluginList, CniAddResult addResult)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "DEL");
                host.AcceptInput(pluginList, pluginOptions, addResult, backwards: true);
                host.ReturnNothing();
            },
            async runtimeOptions =>
            {
                await MemoryInvocationStore.Instance.SetResultAsync(pluginList, addResult, CancellationToken.None);
                await MemoryInvocationStore.Instance.AddAttachmentAsync(
                    new Attachment(runtimeOptions.PluginOptions, pluginList.Plugins[0], pluginList),
                    CancellationToken.None);
                
                var invocation = await CniRuntime.DeletePluginListAsync(pluginList, runtimeOptions, addResult);
                invocation.IsSuccess.Should().BeTrue();
                
                var storedResult =
                    await MemoryInvocationStore.Instance.GetResultAsync(pluginList, CancellationToken.None);
                storedResult.Should().NotBeNull();
                var storedAttachments = await MemoryInvocationStore.Instance
                    .GetAllAttachmentsForPluginListAsync(pluginList, CancellationToken.None);
                storedAttachments.Should().NotBeEmpty();
            },
            disableInvocationStore: true);
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginListAsync_ShouldValidate(PluginList pluginList, CniAddResult addResult)
    {
        await Exec.ValidationTestAsync(CniRuntime.DeleteRequirements,
            r => CniRuntime.DeletePluginListAsync(pluginList, r, addResult));
    }
    
    [Theory, CustomAutoData]
    public async Task DeletePluginListAsync_ShouldThrowForEmpty(PluginList pluginList, CniAddResult addResult)
    {
        pluginList = pluginList with { Plugins = [] };
        await FluentActions
            .Awaiting(
                async () => await CniRuntime.DeletePluginListAsync(pluginList, Exec.EmptyRuntimeOptions, addResult))
            .Should().ThrowAsync<CniEmptyPluginListException>();
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginListWithStoredResultAsync_ShouldThrowForNullStore(PluginList pluginList)
    {
        await FluentActions
            .Awaiting(async () => await CniRuntime
                .DeletePluginListWithStoredResultAsync(pluginList, Exec.EmptyRuntimeOptions))
            .Should().ThrowAsync<CniStoreRetrievalException>();
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginListWithStoredResultAsync_ShouldThrowForDisabledResultStore(PluginList pluginList)
    {
        var runtimeOptions = Exec.EmptyRuntimeOptions with
        {
            InvocationStoreOptions = new InvocationStoreOptions(MemoryInvocationStore.Instance, StoreResults: false)
        };
        await FluentActions
            .Awaiting(async () => await CniRuntime.DeletePluginListWithStoredResultAsync(pluginList, runtimeOptions))
            .Should().ThrowAsync<CniStoreRetrievalException>();
    }

    [Theory, CustomAutoData]
    public async Task DeletePluginListWithStoredResultAsync_ShouldNotThrowForEnabledResultStore(
        PluginList pluginList, CniAddResult addResult)
    {
        await MemoryInvocationStore.Instance.SetResultAsync(pluginList, addResult, CancellationToken.None);
        
        var runtimeOptions = Exec.EmptyRuntimeOptions with
        {
            InvocationStoreOptions = new InvocationStoreOptions(MemoryInvocationStore.Instance, StoreResults: true)
        };
        await FluentActions
            .Awaiting(async () => await CniRuntime.DeletePluginListWithStoredResultAsync(pluginList, runtimeOptions))
            .Should().NotThrowAsync<CniStoreRetrievalException>();
    }
}