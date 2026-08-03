namespace LogLens.Application;

public sealed record LogReadProgress
{
    public string FilePath { get; }

    public long LinesRead { get; }

    public long BytesRead { get; }

    public long TotalBytes { get; }

    public bool IsCompleted { get; }

    public double Percentage
    {
        get
        {
            if (IsCompleted)
            {
                return 100;
            }

            if (TotalBytes == 0)
            {
                return 0;
            }

            return Math.Clamp(
                BytesRead * 100d / TotalBytes,
                0,
                100);
        }
    }

    public LogReadProgress(
        string filePath,
        long linesRead,
        long bytesRead,
        long totalBytes,
        bool isCompleted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (linesRead < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(linesRead));
        }

        if (bytesRead < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesRead));
        }

        if (totalBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalBytes));
        }

        FilePath = filePath;
        LinesRead = linesRead;
        BytesRead = bytesRead;
        TotalBytes = totalBytes;
        IsCompleted = isCompleted;
    }
}
