using System.Diagnostics;
using System.Text;

namespace CniDotNet.Abstractions;

internal sealed class LocalRuntimeHostProcess : IRuntimeHostProcess
{
    private readonly Process _osProcess;
    private readonly StringBuilder _outputBuilder = new();
    public string CurrentOutput => _outputBuilder.ToString();
    
    internal LocalRuntimeHostProcess(Process osProcess)
    {
        _osProcess = osProcess;
        _osProcess.BeginOutputReadLine();
        
        _osProcess.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                _outputBuilder.AppendLine(args.Data);
            }
        };
    }

    public async Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        await _osProcess.WaitForExitAsync(cancellationToken);
    }
}