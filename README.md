# LogLens

Local log analysis and incident diagnostics for developers.

LogLens es una aplicación multiplataforma para procesar archivos de logs, detectar información estructurada, agrupar errores repetidos y diagnosticar incidentes mediante reglas locales.

## Estado

Versión actual: 0.4.0

La solución incluye:

- Modelo de dominio
- Lectura progresiva de archivos
- Procesamiento mediante streaming
- Progreso y cancelación
- Detección de timestamps
- Detección de niveles
- Parser de texto genérico
- Parser de JSON Lines
- Extracción de servicios
- Extracción de códigos HTTP
- Extracción de duración
- Extracción de excepciones
- Extracción de correlation IDs
- CLI
- Aplicación de escritorio con Avalonia
- Pruebas automatizadas

## Formatos admitidos

- `.log`
- `.txt`
- `.jsonl`
- `.ndjson`

## Compilar

```bash
dotnet build LogLens.slnx
```

## Ejecutar pruebas

```bash
dotnet test LogLens.slnx
```

## Mostrar versión

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- version
```

## Leer un archivo

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- read application.log
```

## Configurar la vista previa

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- read application.log --preview 20
```

## Procesar un archivo

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- parse application.log
```

## Procesar JSON Lines

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- parse events.jsonl --preview 20
```

## Ejecutar la aplicación de escritorio

```bash
dotnet run \
  --project src/LogLens.Desktop/LogLens.Desktop.csproj
```

## Datos detectados

LogLens puede extraer:

- Fecha y hora
- Nivel
- Mensaje
- Servicio
- Excepción
- Código HTTP
- Duración
- Correlation ID
- Request ID
- Trace ID
- Metadatos JSON