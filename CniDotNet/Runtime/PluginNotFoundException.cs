namespace CniDotNet.Runtime;

public sealed class PluginNotFoundException(string message) : Exception(message);