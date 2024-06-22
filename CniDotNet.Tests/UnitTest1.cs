using CniDotNet.Abstractions;
using CniDotNet.Data;

namespace CniDotNet.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var configs =
            await NetworkConfigurationHandler.LookupFirstAsync(LocalFilesystem.Current,
                new LookupOptions([".conflist"], Directory: "/etc/cni/net.d"));
        Console.WriteLine(configs);
    }
}