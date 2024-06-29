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
    public async Task AddPluginAsync_ShouldNotCacheWhenNotAsked(CniAddResult addResult, Plugin plugin)
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
}