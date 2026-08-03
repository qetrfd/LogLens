using LogLens.Core;

namespace LogLens.Application;

public interface ILogLineParser
{
    string Name { get; }

    int Priority { get; }

    LogParseResult Parse(
        RawLogLine line,
        LogParserContext context);
}