using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Invocations;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using CniDotNet.Runtime.Exceptions;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class RuntimeGcTests
{
    [Theory, CustomAutoData]
    public async Task GcSuite_ManualPlugins_ManualAttachments(IReadOnlyList<Attachment> attachments,
        IReadOnlyList<Plugin> plugins, CniErrorResult errorResult)
    {
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.ReturnNothing();
            },
            r => CniRuntime.GarbageCollectAsync(plugins, attachments, r));
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult with { Code = 4 });
            },
            r => CniRuntime.GarbageCollectAsync(plugins, attachments, r));
        await GcErrorAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult);
            },
            r => CniRuntime.GarbageCollectAsync(plugins, attachments, r),
            errorResult, plugins[0]);
    }

    [Theory, CustomAutoData]
    public async Task GcSuite_PluginList_ManualAttachments(IReadOnlyList<Attachment> attachments,
        PluginList pluginList, CniErrorResult errorResult)
    {
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(pluginList.Plugins, options, attachments);
                host.ReturnNothing();
            },
            r => CniRuntime.GarbageCollectAsync(pluginList, attachments, r));
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(pluginList.Plugins, options, attachments);
                host.Return(errorResult with { Code = 4 });
            },
            r => CniRuntime.GarbageCollectAsync(pluginList, attachments, r));
        await GcErrorAsync(
            (host, options) =>
            {
                host.AcceptInput(pluginList.Plugins, options, attachments);
                host.Return(errorResult);
            },
            r => CniRuntime.GarbageCollectAsync(pluginList, attachments, r),
            errorResult, pluginList.Plugins[0]);
    }

    [Theory, CustomAutoData]
    public async Task GcSuite_PluginLists_ManualAttachments(IReadOnlyList<PluginList> pluginLists,
        IReadOnlyList<Attachment> attachments, CniErrorResult errorResult)
    {
        var plugins = FlattenPluginLists(pluginLists);
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.ReturnNothing();
            },
            r => CniRuntime.GarbageCollectAsync(plugins, attachments, r));
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult with { Code = 4 });
            },
            r => CniRuntime.GarbageCollectAsync(plugins, attachments, r));
        await GcErrorAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult);
            },
            r => CniRuntime.GarbageCollectAsync(plugins, attachments, r),
            errorResult, plugins[0]);
    }

    [Theory, CustomAutoData]
    public async Task GcSuite_ManualPlugins_StoredAttachments(IReadOnlyList<Plugin> plugins,
        IReadOnlyList<Attachment> attachments, CniErrorResult errorResult)
    {
        await BootstrapStoreAsync(attachments);
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.ReturnNothing();
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r));
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult with { Code = 4 });
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r));
        await GcErrorAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult);
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r),
            errorResult, plugins[0]);
        await EnsureThrowsNotFoundAsync(r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r));
    }

    [Theory, CustomAutoData]
    public async Task GcSuite_PluginList_StoredAttachments(PluginList pluginList, IReadOnlyList<Attachment> attachments,
        CniErrorResult errorResult)
    {
        await BootstrapStoreAsync(attachments);
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(pluginList.Plugins, options, attachments);
                host.ReturnNothing();
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(pluginList, r));
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(pluginList.Plugins, options, attachments);
                host.Return(errorResult with { Code = 4 });
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(pluginList, r));
        await GcErrorAsync(
            (host, options) =>
            {
                host.AcceptInput(pluginList.Plugins, options, attachments);
                host.Return(errorResult);
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(pluginList, r),
            errorResult, pluginList.Plugins[0]);
        await EnsureThrowsNotFoundAsync(r => CniRuntime.GarbageCollectWithStoreAsync(pluginList, r));
    }

    [Theory, CustomAutoData]
    public async Task GcSuite_PluginLists_StoredAttachments(IReadOnlyList<PluginList> pluginLists,
        IReadOnlyList<Attachment> attachments, CniErrorResult errorResult)
    {
        var plugins = FlattenPluginLists(pluginLists);
        await BootstrapStoreAsync(attachments);
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.ReturnNothing();
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r));
        await GcSuccessAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult with { Code = 4 });
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r));
        await GcErrorAsync(
            (host, options) =>
            {
                host.AcceptInput(plugins, options, attachments);
                host.Return(errorResult);
            },
            r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r),
            errorResult, plugins[0]);
        await EnsureThrowsNotFoundAsync(r => CniRuntime.GarbageCollectWithStoreAsync(plugins, r));
    }

    private static async Task BootstrapStoreAsync(IReadOnlyList<Attachment> attachments)
    {
        await MemoryInvocationStore.Instance.ClearAsync(CancellationToken.None);
        foreach (var attachment in attachments)
        {
            await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        }
    }

    private static async Task EnsureThrowsNotFoundAsync(Func<RuntimeOptions, Task<PluginListInvocation>> call)
    {
        IEnumerable<RuntimeOptions> runtimeOptionList =
            [
                new RuntimeOptions(null!, null!, null!), 
                new RuntimeOptions(null!, null!, null!, 
                    new InvocationStoreOptions(MemoryInvocationStore.Instance, StoreAttachments: false))
            ];
        
        foreach (var runtimeOptions in runtimeOptionList)
        {
            await FluentActions
                .Awaiting(() => call(runtimeOptions))
                .Should().ThrowAsync<CniStoreRetrievalException>();
        }
    }

    private static List<Plugin> FlattenPluginLists(IReadOnlyList<PluginList> pluginLists)
    {
        return pluginLists
            .SelectMany(l => l.Plugins)
            .DistinctBy(p => p.Type)
            .ToList();
    }

    private static async Task GcSuccessAsync(Action<TestRuntimeHost, PluginOptions> configureHost,
        Func<RuntimeOptions, Task<PluginListInvocation>> call)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "GC");
                configureHost(host, pluginOptions);
            },
            async runtimeOptions =>
            {
                var invocation = await call(runtimeOptions);
                invocation.IsSuccess.Should().BeTrue();
                invocation.IsError.Should().BeFalse();
                invocation.ErrorResult.Should().BeNull();
                invocation.ErrorCausePlugin.Should().BeNull();
            },
            shouldClear: false);
    }
    
    private static async Task GcErrorAsync(Action<TestRuntimeHost, PluginOptions> configureHost,
        Func<RuntimeOptions, Task<PluginListInvocation>> call, CniErrorResult errorResult, Plugin causePlugin)
    {
        await Exec.RuntimeTestAsync(
            (host, pluginOptions) =>
            {
                host.AcceptEnvironment("CNI_COMMAND", "GC");
                configureHost(host, pluginOptions);
            },
            async runtimeOptions =>
            {
                var invocation = await call(runtimeOptions);
                invocation.IsSuccess.Should().BeFalse();
                invocation.IsError.Should().BeTrue();
                invocation.ErrorResult.Should().BeEquivalentTo(errorResult);
                invocation.ErrorCausePlugin.Should().Be(causePlugin);
            },
            shouldClear: false);
    }
}