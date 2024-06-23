using System.Diagnostics;
using System.Text;

namespace CniDotNet.Host;

public class LocalCniHostProcess : ICniHostProcess
{
    private readonly Process _osProcess;
    private readonly StringBuilder _outputBuilder = new();
    
    internal LocalCniHostProcess(Process osProcess)
    {
        _osProcess = osProcess;
        _osProcess.BeginOutputReadLine();

        _osProcess.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null) _outputBuilder.Append(args.Data);
        };
    }
    
    public async Task WriteLineAsync(string line, CancellationToken cancellationToken)
    {
        await _osProcess.StandardInput.WriteLineAsync(new StringBuilder(line), cancellationToken);
    }

    public async Task<string> WaitForExitAsync(CancellationToken cancellationToken)
    {
        await _osProcess.WaitForExitAsync(cancellationToken);
        return _outputBuilder.ToString();
    }
}