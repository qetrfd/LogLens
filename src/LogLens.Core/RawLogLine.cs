namespace LogLens.Core;

public sealed record RawLogLine
{
    public Guid SourceId { get; }

    public long LineNumber { get; }

    public string Text { get; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Text);

    public RawLogLine(
        Guid sourceId,
        long lineNumber,
        string text)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la fuente no puede estar vacío.",
                nameof(sourceId));
        }

        if (lineNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lineNumber),
                "El número de línea debe ser mayor que cero.");
        }

        ArgumentNullException.ThrowIfNull(text);

        SourceId = sourceId;
        LineNumber = lineNumber;
        Text = text;
    }
}
