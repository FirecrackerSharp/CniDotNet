namespace CniDotNet.Data;

public sealed record NetworkNamespace(
    string Name,
    uint? Id = null)
{
    public string Path => $"/var/run/netns/{Name}";
}