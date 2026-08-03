using LogLens.Core;

namespace LogLens.Application;

public interface ILogFingerprintGenerator
{
    LogFingerprint Generate(
        ParsedLogLine line);
}