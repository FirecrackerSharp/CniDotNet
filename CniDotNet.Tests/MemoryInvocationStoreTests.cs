using CniDotNet.Abstractions;
using CniDotNet.Data;
using CniDotNet.Data.CniResults;
using CniDotNet.Data.Options;
using CniDotNet.Tests.Helpers;
using FluentAssertions;

namespace CniDotNet.Tests;

public class MemoryInvocationStoreTests
{
    [Theory, CustomAutoData]
    public async Task BinaryLocation_ShouldSetAndGet(string pluginType, string binaryLocation)
    {
        await MemoryInvocationStore.Instance.SetBinaryLocationAsync(pluginType, binaryLocation, CancellationToken.None);
        var retrievedLocation =
            await MemoryInvocationStore.Instance.GetBinaryLocationAsync(pluginType, CancellationToken.None);
        retrievedLocation.Should().Be(binaryLocation);
    }

    [Theory, CustomAutoData]
    public async Task BinaryLocation_ShouldNotGetNonExistent(string pluginType)
    {
        var location = await MemoryInvocationStore.Instance.GetBinaryLocationAsync(pluginType, CancellationToken.None);
        location.Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task Attachment_ShouldAddAndGet(Attachment attachment)
    {
        await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        var retrievedAttachment = await MemoryInvocationStore.Instance.GetAttachmentAsync(attachment.Plugin,
            attachment.PluginOptions, CancellationToken.None);
        retrievedAttachment.Should().Be(attachment);
    }

    [Theory, CustomAutoData]
    public async Task Attachment_ShouldNotGetNonExistent(Plugin plugin, PluginOptions pluginOptions)
    {
        var attachment =
            await MemoryInvocationStore.Instance.GetAttachmentAsync(plugin, pluginOptions, CancellationToken.None);
        attachment.Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task Attachment_ShouldAddAndRemove(Attachment attachment)
    {
        await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        await MemoryInvocationStore.Instance.RemoveAttachmentAsync(attachment.Plugin, attachment.PluginOptions,
            CancellationToken.None);
        var retrievedAttachment = await MemoryInvocationStore.Instance.GetAttachmentAsync(attachment.Plugin,
            attachment.PluginOptions, CancellationToken.None);
        retrievedAttachment.Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task Attachment_ShouldRetrieveForPlugin(IEnumerable<Attachment> attachments, Plugin plugin)
    {
        attachments = attachments.Select(x => x with { Plugin = plugin }).ToList();
        foreach (var attachment in attachments)
        {
            await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        }

        var retrievedAttachments =
            await MemoryInvocationStore.Instance.GetAllAttachmentsForPluginAsync(plugin, CancellationToken.None);
        retrievedAttachments.Should().BeEquivalentTo(attachments);
    }

    [Theory, CustomAutoData]
    public async Task Attachment_ShouldRetrieveForPluginList(IEnumerable<Attachment> attachments, PluginList pluginList)
    {
        attachments = attachments.Select(x => x with { ParentPluginList = pluginList }).ToList();
        foreach (var attachment in attachments)
        {
            await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        }

        var retrievedAttachments =
            await MemoryInvocationStore.Instance.GetAllAttachmentsForPluginListAsync(pluginList, CancellationToken.None);
        retrievedAttachments.Should().BeEquivalentTo(attachments);
    }

    [Theory, CustomAutoData]
    public async Task Attachment_ShouldRetrieveAll(List<Attachment> attachments)
    {
        foreach (var attachment in attachments)
        {
            await MemoryInvocationStore.Instance.AddAttachmentAsync(attachment, CancellationToken.None);
        }

        var retrievedAttachments = await MemoryInvocationStore.Instance.GetAllAttachmentsAsync(CancellationToken.None);
        attachments.Should().BeSubsetOf(retrievedAttachments);
    }

    [Theory, CustomAutoData]
    public async Task Result_ShouldSetAndGet(PluginList pluginList, CniAddResult result)
    {
        await MemoryInvocationStore.Instance.SetResultAsync(pluginList, result, CancellationToken.None);
        var retrievedResult = await MemoryInvocationStore.Instance.GetResultAsync(pluginList, CancellationToken.None);
        retrievedResult.Should().BeEquivalentTo(result);
    }

    [Theory, CustomAutoData]
    public async Task Result_ShouldNotGetNonExistent(PluginList pluginList)
    {
        var result = await MemoryInvocationStore.Instance.GetResultAsync(pluginList, CancellationToken.None);
        result.Should().BeNull();
    }

    [Theory, CustomAutoData]
    public async Task Result_ShouldSetAndRemove(PluginList pluginList, CniAddResult result)
    {
        await MemoryInvocationStore.Instance.SetResultAsync(pluginList, result, CancellationToken.None);
        await MemoryInvocationStore.Instance.RemoveResultAsync(pluginList, CancellationToken.None);
        var retrievedResult = await MemoryInvocationStore.Instance.GetResultAsync(pluginList, CancellationToken.None);
        retrievedResult.Should().BeNull();
    }
}