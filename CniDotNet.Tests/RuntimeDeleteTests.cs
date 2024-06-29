using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Runtime;
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
}