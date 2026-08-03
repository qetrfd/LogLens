namespace LogLens.Application;

public sealed record LogParserContext
{
    public Guid SourceId { get; }

    public string SourceName { get; }

    public string FilePath { get; }

    public LogParserContext(
        Guid sourceId,
        string sourceName,
        string filePath)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la fuente no puede estar vacío.",
                nameof(sourceId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        SourceId = sourceId;
        SourceName = sourceName.Trim();
        FilePath = Path.GetFullPath(filePath.Trim());
    }
}