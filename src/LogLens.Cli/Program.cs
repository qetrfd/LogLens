using LogLens.Application;
using LogLens.Infrastructure;

StartupSummaryService startupService = new(
    new RuntimeEnvironmentProvider());

StartupSummary summary = startupService.Create();

Console.WriteLine();
Console.WriteLine("LOGLENS");
Console.WriteLine(new string('─', 58));
Console.WriteLine($"Versión:       {summary.Product.Version}");
Console.WriteLine($"Descripción:   {summary.Product.Description}");
Console.WriteLine($"Sistema:       {summary.Runtime.OperatingSystem}");
Console.WriteLine($"Arquitectura:  {summary.Runtime.Architecture}");
Console.WriteLine($"Runtime:       {summary.Runtime.Framework}");
Console.WriteLine(new string('─', 58));
Console.WriteLine("Estado: LogLens está listo para comenzar.");
Console.WriteLine();

return 0;