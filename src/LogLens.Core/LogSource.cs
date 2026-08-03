namespace LogLens.Core;

public sealed record LogSource
{
    public Guid Id { get; }

    public string Name { get; }

    public string Location { get; }

    public LogSourceKind Kind { get; }

    public bool IsLive { get; }

    public LogSource(
        Guid id,
        string name,
        string location,
        LogSourceKind kind,
        bool isLive)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la fuente no puede estar vacío.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        Id = id;
        Name = name.Trim();
        Location = location.Trim();
        Kind = kind;
        IsLive = isLive;
    }
}
