namespace LogLens.Core;

public sealed record DiagnosticEvidence
{
    public string Code { get; }

    public string Label { get; }

    public string Value { get; }

    public DiagnosticEvidence(
        string code,
        string label,
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Code = NormalizeCode(code);
        Label = label.Trim();
        Value = value.Trim();
    }

    private static string NormalizeCode(
        string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-');
    }
}