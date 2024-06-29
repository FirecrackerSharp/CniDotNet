using System.Diagnostics;
using System.Text;
using CniDotNet.Abstractions;
using CniDotNet.Data.Options;

namespace CniDotNet.Tests.Helpers;

public static class Exec
{
    public static async Task<string> CommandAsync(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("/bin/su")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            }
        };

        process.Start();
        
        var buffer = new StringBuilder();
        process.BeginOutputReadLine();

        await process.StandardInput.WriteLineAsync(Environment.GetEnvironmentVariable("ROOT_PWD"));
        await process.StandardInput.WriteLineAsync($"{command} ; exit");
        
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                buffer.Append(args.Data);
            }
        };
        
        await process.WaitForExitAsync();
        return buffer.ToString();
    }
    
    public static async Task RuntimeTestAsync(
        Action<TestRuntimeHost, PluginOptions> configureHost,
        Func<RuntimeOptions, Task> act,
        bool disableInvocationStore = false)
    {
        await MemoryInvocationStore.Instance.ClearAsync(CancellationToken.None);
        
        var namespaceName = Guid.NewGuid().ToString();
        var pluginOptions = new PluginOptions("1.0.0", "fcnet", "fcnet", namespaceName, "eth0");
        
        var host = new TestRuntimeHost();
        configureHost(host, pluginOptions);
        host.AcceptEnvironment(pluginOptions);

        var runtimeOptions = new RuntimeOptions(
            pluginOptions,
            new InvocationOptions(host),
            new PluginSearchOptions(Directory: "/tmp"),
            disableInvocationStore ? null : new InvocationStoreOptions(MemoryInvocationStore.Instance));

        await CommandAsync($"ip netns add {namespaceName}");
        await act(runtimeOptions);
        await CommandAsync($"ip netns del {namespaceName}");
    }
}