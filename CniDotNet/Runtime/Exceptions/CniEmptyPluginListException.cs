namespace CniDotNet.Runtime.Exceptions;

public sealed class CniEmptyPluginListException()
    : Exception("The given plugin list was empty, thus no operation could be performed");