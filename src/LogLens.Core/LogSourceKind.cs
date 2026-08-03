namespace LogLens.Core;

public enum LogSourceKind
{
    GenericFile = 0,
    JsonLines = 1,
    AspNet = 2,
    NodeJs = 3,
    Docker = 4,
    NginxAccess = 5,
    NginxError = 6,
    Java = 7,
    Python = 8,
    Rust = 9,
    Firebase = 10,
    CloudRun = 11,
    SystemJournal = 12,
    WindowsEventLog = 13,
    MacUnifiedLog = 14,
    Unknown = 15
}
