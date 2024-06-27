namespace CniDotNet.Runtime.Exceptions;

public sealed class CniValidationFailureException(string message) : Exception(message);