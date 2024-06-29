using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Runtime;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class RuntimeAddTests
{
    [Theory, CustomAutoData]
    public async Task AddPluginAsync_ShouldHandleSuccess(CniAddResult addResult, Plugin plugin)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "ADD");
                host.AcceptInput(plugin, pluginOptions);
                host.Return(addResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.AddPluginAsync(plugin, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
                
                invocation.SuccessAttachment.Should().NotBeNull();
                invocation.SuccessAttachment!.Plugin.Should().Be(plugin);
                invocation.SuccessAttachment!.PluginOptions.Should().Be(runtimeOptions.PluginOptions);
                invocation.SuccessAttachment!.ParentPluginList.Should().BeNull();
                invocation.SuccessAddResult.Should().BeEquivalentTo(addResult);

                var addedAttachment = await MemoryInvocationStore.Instance
                    .GetAttachmentAsync(plugin, runtimeOptions.PluginOptions, CancellationToken.None);
                addedAttachment.Should().Be(invocation.SuccessAttachment);
            });
    }


    [Theory, CustomAutoData]
    public async Task AddPluginAsync_ShouldHandleError(CniErrorResult errorResult, Plugin plugin)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "ADD");
                host.AcceptInput(plugin, pluginOptions);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.AddPluginAsync(plugin, runtimeOptions);
                invocation.IsError.Should().BeTrue();
                invocation.IsSuccess.Should().BeFalse();
                invocation.SuccessAttachment.Should().BeNull();
                invocation.SuccessAddResult.Should().BeNull();

                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
            });
    }

    [Theory, CustomAutoData]
    public async Task AddPluginAsync_ShouldHandleDisabledStore(CniAddResult addResult, Plugin plugin)
    {
        await Exec.RuntimeTestAsync(
            (host, _) => host.Return(addResult),
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.AddPluginAsync(plugin, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.SuccessAttachment.Should().NotBeNull();

                var storedAttachment = await MemoryInvocationStore.Instance
                    .GetAttachmentAsync(plugin, runtimeOptions.PluginOptions, CancellationToken.None);
                storedAttachment.Should().BeNull();
            },
            disableInvocationStore: true);
    }

    [Theory, CustomAutoData]
    public async Task AddPluginAsync_ShouldValidate(Plugin plugin)
    {
        await Exec.ValidationTestAsync(CniRuntime.AddRequirements, r => CniRuntime.AddPluginAsync(plugin, r));
    }

    [Theory, CustomAutoData]
    public async Task AddPluginListAsync_ShouldHandleSuccess(CniAddResult addResult, PluginList pluginList)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "ADD");
                host.AcceptInput(pluginList, pluginOptions, addResult, skipFirst: true);
                host.Return(addResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.AddPluginListAsync(pluginList, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
                invocation.ErrorCausePlugin.Should().BeNull();

                var expectedAttachments = pluginList.Plugins
                    .Select(p => new Attachment(runtimeOptions.PluginOptions, p, pluginList))
                    .ToList();
                invocation.SuccessAttachments.Should().BeEquivalentTo(expectedAttachments);
                invocation.SuccessAddResult.Should().BeEquivalentTo(addResult);

                var storedAttachments = await MemoryInvocationStore.Instance
                    .GetAllAttachmentsForPluginListAsync(pluginList, CancellationToken.None);
                storedAttachments.Should().BeEquivalentTo(expectedAttachments);
                var storedResult =
                    await MemoryInvocationStore.Instance.GetResultAsync(pluginList, CancellationToken.None);
                storedResult.Should().BeEquivalentTo(addResult);
            });
    }

    [Theory, CustomAutoData]
    public async Task AddPluginListAsync_ShouldHandleError(CniErrorResult errorResult, CniAddResult addResult,
        PluginList pluginList)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "ADD");
                host.AcceptInput(pluginList, pluginOptions, addResult, skipFirst: true);
                host.Return(errorResult);
            },
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.AddPluginListAsync(pluginList, runtimeOptions);
                invocation.IsError.Should().BeTrue();
                invocation.IsSuccess.Should().BeFalse();
                invocation.SuccessAddResult.Should().BeNull();
                invocation.SuccessAttachments.Should().BeNull();

                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
                invocation.ErrorCausePlugin.Should().Be(pluginList.Plugins[0]);
            });
    }

    [Theory, CustomAutoData]
    public async Task AddPluginListAsync_ShouldHandleDisabledStore(CniAddResult addResult, PluginList pluginList)
    {
        await Exec.RuntimeTestAsync(
            (host, _) => host.Return(addResult),
            async runtimeOptions =>
            {
                var invocation = await CniRuntime.AddPluginListAsync(pluginList, runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.SuccessAddResult.Should().BeEquivalentTo(addResult);
                invocation.SuccessAttachments.Should().NotBeNull();

                var storedAttachments = await MemoryInvocationStore.Instance
                    .GetAllAttachmentsForPluginListAsync(pluginList, CancellationToken.None);
                storedAttachments.Should().BeEmpty();
                var storedResult =
                    await MemoryInvocationStore.Instance.GetResultAsync(pluginList, CancellationToken.None);
                storedResult.Should().BeNull();
            },
            disableInvocationStore: true);
    }
}