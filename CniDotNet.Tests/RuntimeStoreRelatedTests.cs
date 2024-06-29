using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class RuntimeStoreRelatedTests
{
    private readonly RuntimeOptions _runtimeOptions =
        new(null!, null!, null!, new InvocationStoreOptions(MemoryInvocationStore.Instance));

    private readonly RuntimeOptions _invalidRuntimeOptions1 =
        new(null!, null!, null!);

    private readonly RuntimeOptions _invalidRuntimeOptions2 =
        new(null!, null!, null!,
            new InvocationStoreOptions(MemoryInvocationStore.Instance, StoreAttachments: false, StoreResults: false,
                StoreBinaryLocations: false));

    [Theory, CustomAutoData]
    public async Task GetStoredResultAsync_ShouldReturnOne(PluginList pluginList, CniAddResult expectedResult)
    {
        await MemoryInvocationStore.Instance.SetResultAsync(pluginList, expectedResult, CancellationToken.None);
        var actualResult = await CniRuntime.GetStoredResultAsync(pluginList, _runtimeOptions);
        actualResult.Should().Be(expectedResult);
    }

    [Theory, CustomAutoData]
    public async Task GetStoredResultAsync_ShouldRejectInvalidRuntimeOptions(PluginList pluginList, CniAddResult expectedResult)
    {
        await MemoryInvocationStore.Instance.SetResultAsync(pluginList, expectedResult, CancellationToken.None);
        (await CniRuntime.GetStoredResultAsync(pluginList, _invalidRuntimeOptions1)).Should().BeNull();
        (await CniRuntime.GetStoredResultAsync(pluginList, _invalidRuntimeOptions2)).Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task GetStoredAttachmentAsync_ShouldReturnOne(Attachment attachment)
    {
        await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        var actualAttachment = await CniRuntime.GetStoredAttachmentAsync(attachment.Plugin, attachment.PluginOptions,
            _runtimeOptions);
        actualAttachment.Should().Be(attachment);
    }

    [Theory, CustomAutoData]
    public async Task GetStoredAttachmentAsync_ShouldRejectInvalidRuntimeOptions(Attachment attachment)
    {
        await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        (await CniRuntime.GetStoredAttachmentAsync(attachment.Plugin, attachment.PluginOptions,
            _invalidRuntimeOptions1)).Should().BeNull();
        (await CniRuntime.GetStoredAttachmentAsync(attachment.Plugin, attachment.PluginOptions,
            _invalidRuntimeOptions2)).Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task GetStoredAttachmentsAsync_ShouldReturnPluginListMatches(IEnumerable<Attachment> attachmentsBase1,
        IEnumerable<Attachment> attachmentsBase2, PluginList pluginList, Plugin plugin)
    {
        var attachments1 = attachmentsBase1
            .Select(a => a with { ParentPluginList = pluginList }).ToList();
        foreach (var attachment in attachments1)
        {
            await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        }
        var attachments2 = attachmentsBase2
            .Select(a => a with { Plugin = plugin }).ToList();
        foreach (var attachment in attachments2)
        {
            await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        }
        
        var actualAttachments1 = await CniRuntime.GetStoredAttachmentsAsync(pluginList, _runtimeOptions);
        actualAttachments1.Should().BeEquivalentTo(attachments1);
        var actualAttachments2 = await CniRuntime.GetStoredAttachmentsAsync(plugin, _runtimeOptions);
        actualAttachments2.Should().BeEquivalentTo(attachments2);
    }

    [Theory, CustomAutoData]
    public async Task GetStoredAttachmentsAsync_ShouldRejectInvalidRuntimeOptions(PluginList pluginList, Plugin plugin)
    {
        (await CniRuntime.GetStoredAttachmentsAsync(pluginList, _invalidRuntimeOptions1)).Should().BeNull();
        (await CniRuntime.GetStoredAttachmentsAsync(pluginList, _invalidRuntimeOptions2)).Should().BeNull();
        (await CniRuntime.GetStoredAttachmentsAsync(plugin, _invalidRuntimeOptions1)).Should().BeNull();
        (await CniRuntime.GetStoredAttachmentsAsync(plugin, _invalidRuntimeOptions2)).Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task GetAllStoredAttachmentsAsync_ShouldReturnMatches(List<Attachment> attachments)
    {
        await MemoryInvocationStore.Instance.ClearAsync(CancellationToken.None);
        foreach (var attachment in attachments)
        {
            await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        }

        var actualAttachments = await CniRuntime.GetAllStoredAttachmentsAsync(_runtimeOptions);
        actualAttachments.Should().BeEquivalentTo(attachments);
    }

    [Fact]
    public async Task GetAllStoredAttachmentsAsync_ShouldRejectInvalidRuntimeOptions()
    {
        (await CniRuntime.GetAllStoredAttachmentsAsync(_invalidRuntimeOptions1)).Should().BeNull();
        (await CniRuntime.GetAllStoredAttachmentsAsync(_invalidRuntimeOptions2)).Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task GetStoredBinaryLocationAsync_ShouldReturnMatch(Plugin plugin, string location)
    {
        await MemoryInvocationStore.Instance.SetBinaryLocationAsync(plugin.Type, location, CancellationToken.None);
        var actualLocation1 = await CniRuntime.GetStoredBinaryLocationAsync(plugin, _runtimeOptions);
        var actualLocation2 = await CniRuntime.GetStoredBinaryLocationAsync(plugin.Type, _runtimeOptions);
        actualLocation1.Should().Be(actualLocation2).And.Be(location);
    }

    [Theory, CustomAutoData]
    public async Task GetStoredBinaryLocationAsync_ShouldRejectInvalidRuntimeOptions(Plugin plugin, string location)
    {
        await MemoryInvocationStore.Instance.SetBinaryLocationAsync(plugin.Type, location, CancellationToken.None);
        (await CniRuntime.GetStoredBinaryLocationAsync(plugin, _invalidRuntimeOptions1)).Should().BeNull();
        (await CniRuntime.GetStoredBinaryLocationAsync(plugin.Type, _invalidRuntimeOptions1)).Should().BeNull();
        (await CniRuntime.GetStoredBinaryLocationAsync(plugin, _invalidRuntimeOptions2)).Should().BeNull();
        (await CniRuntime.GetStoredBinaryLocationAsync(plugin.Type, _invalidRuntimeOptions2)).Should().BeNull();
    }
}