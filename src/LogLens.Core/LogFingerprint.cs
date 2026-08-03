namespace LogLens.Core;

public sealed record LogFingerprint
{
    public string Value { get; }

    public string NormalizedMessage { get; }

    public LogFingerprint(
        string value,
        string normalizedMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            normalizedMessage);

        Value = value.Trim().ToLowerInvariant();
        NormalizedMessage = normalizedMessage.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}