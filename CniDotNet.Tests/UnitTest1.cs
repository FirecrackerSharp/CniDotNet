using CniDotNet.Abstractions;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var c =
            await NetworkConfigurationParser.LoadFromFileAsync(LocalFilesystem.Current, "/etc/cni/net.d/fcnet.conflist");
    }
}