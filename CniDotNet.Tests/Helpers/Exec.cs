using System.Diagnostics;
using System.Text;
using CniDotNet.Abstractions;
using CniDotNet.Data.Invocations;
using CniDotNet.Data.Options;
using CniDotNet.Runtime;
using CniDotNet.Runtime.Exceptions;
using FluentAssertions;

namespace CniDotNet.Tests.Helpers;

public static class Exec
{
    private static readonly PluginOptions PluginOptions = new("1.0.0", "fcnet", "fcnet", "netns", "eth0");
    
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

    public static async Task ValidationTestAsync<T>(
        PluginOptionRequirement requirements,
        Func<RuntimeOptions, Task<T>> invoker) where T : IBaseInvocation
    {
        // skip validation
        await AcceptAsync(PluginOptions with
        {
            CniVersion = null!, Name = null!, ContainerId = null, InterfaceName = null, NetworkNamespace = null,
            SkipValidation = true
        });

        // path
        if (requirements.HasFlag(PluginOptionRequirement.Path))
        {
            await RejectAsync(PluginOptions with { IncludePath = false });
            await AcceptAsync(PluginOptions with { IncludePath = true });
        }

        // network namespace
        if (requirements.HasFlag(PluginOptionRequirement.NetworkNamespace))
        {
            await RejectAsync(PluginOptions with { NetworkNamespace = null });
            await RejectAsync(PluginOptions with { NetworkNamespace = " " });
            await AcceptAsync(PluginOptions with { NetworkNamespace = "validNetNs" });
        }
        
        // network name
        await RejectAsync(PluginOptions with { Name = null! });
        await RejectAsync(PluginOptions with { Name = " " });
        await RejectAsync(PluginOptions with { Name = "not matching regex" });
        await AcceptAsync(PluginOptions with { Name = "validName" });
        
        // container ID
        if (requirements.HasFlag(PluginOptionRequirement.ContainerId))
        {
            await RejectAsync(PluginOptions with { ContainerId = null });
            await RejectAsync(PluginOptions with { ContainerId = "  " });
            await AcceptAsync(PluginOptions with { ContainerId = "validContainerId" });
        }
        await RejectAsync(PluginOptions with { ContainerId = "not matching regex" });
        
        // interface name
        if (requirements.HasFlag(PluginOptionRequirement.InterfaceName))
        {
            await RejectAsync(PluginOptions with { InterfaceName = null });
        }
        await RejectAsync(PluginOptions with { InterfaceName = new string('p', 999) });
        await RejectAsync(PluginOptions with { InterfaceName = "." });
        await RejectAsync(PluginOptions with { InterfaceName = ".." });
        await RejectAsync(PluginOptions with { InterfaceName = "ifnamewitha/" });
        await RejectAsync(PluginOptions with { InterfaceName = "ifnamewitha:" });
        await RejectAsync(PluginOptions with { InterfaceName = "ifnamewitha " });
        await AcceptAsync(PluginOptions with { InterfaceName = "validifname" });

        return;

        async Task AcceptAsync(PluginOptions pluginOptions)
        {
            await FluentActions
                .Awaiting(async () => await invoker(new RuntimeOptions(pluginOptions, null!, null!)))
                .Should().NotThrowAsync<CniValidationFailureException>();
        }

        async Task RejectAsync(PluginOptions pluginOptions)
        {
            await FluentActions
                .Awaiting(async () => await invoker(new RuntimeOptions(pluginOptions, null!, null!)))
                .Should().ThrowAsync<CniValidationFailureException>();
        }
    }
}