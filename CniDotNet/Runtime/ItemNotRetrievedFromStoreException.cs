namespace CniDotNet.Runtime;

public sealed class ItemNotRetrievedFromStoreException(string message) : Exception(message);