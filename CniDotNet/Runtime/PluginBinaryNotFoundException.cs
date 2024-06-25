namespace CniDotNet.Runtime;

public sealed class PluginBinaryNotFoundException(string message) : Exception(message);