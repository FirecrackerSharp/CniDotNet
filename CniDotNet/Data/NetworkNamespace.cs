namespace CniDotNet.Data;

public sealed record NetworkNamespace(
    string Name,
    uint? Id)
{
    public string Path => $"/var/run/netns/{Name}";
}