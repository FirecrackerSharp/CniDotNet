using CniDotNet.Abstractions;

namespace CniDotNet.Data.Options;

public record InvocationStoreOptions(
    IInvocationStore InvocationStore,
    bool StoreAttachments = true,
    bool StoreBinaryLocations = true,
    bool StoreResults = true);