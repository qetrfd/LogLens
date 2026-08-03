# LogLens

Local log analysis and incident diagnostics for developers.

LogLens es una aplicación multiplataforma para procesar archivos de logs, agrupar errores repetidos, detectar incidentes y explicar problemas técnicos mediante reglas locales.

## Estado

Versión actual: 0.3.0

La solución incluye:

- Modelo de dominio
- Lectura progresiva de archivos
- Procesamiento mediante streaming
- Progreso y cancelación
- CLI
- Aplicación de escritorio con Avalonia
- Pruebas automatizadas

## Formatos admitidos

- `.log`
- `.txt`
- `.jsonl`
- `.ndjson`

## Leer un archivo

```bash
dotnet run --project src/LogLens.Cli/LogLens.Cli.csproj -- read archivo.log
```

## Elegir cantidad de líneas de vista previa

```bash
dotnet run --project src/LogLens.Cli/LogLens.Cli.csproj -- read archivo.log --preview 20
```

## Ejecutar la aplicación

```bash
dotnet run --project src/LogLens.Desktop/LogLens.Desktop.csproj
```

## Compilar

```bash
dotnet build LogLens.slnx
```

## Ejecutar pruebas

```bash
dotnet test LogLens.slnx
```