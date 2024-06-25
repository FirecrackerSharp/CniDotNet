namespace CniDotNet.Runtime;

public sealed class PluginOptionValidationException(string message) : Exception(message);