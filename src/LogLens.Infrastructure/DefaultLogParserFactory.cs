using LogLens.Application;

namespace LogLens.Infrastructure;

public static class DefaultLogParserFactory
{
    public static ILogLineParser Create()
    {
        return new CompositeLogLineParser(
        [
            new JsonLineLogParser(),
            new GenericTextLogParser()
        ]);
    }
}