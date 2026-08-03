# LogLens

Local log analysis and incident diagnostics for developers.

LogLens es una aplicación multiplataforma para leer, procesar, estructurar y agrupar archivos de logs sin cargar todo el archivo en memoria.

## Estado

Versión actual: 0.5.0

La solución incluye:

- Modelo de dominio
- Lectura progresiva mediante streaming
- Progreso y cancelación
- Parser de texto genérico
- Parser de JSON Lines
- Detección de timestamps
- Detección de niveles
- Extracción de servicios
- Extracción de códigos HTTP
- Extracción de duración
- Extracción de excepciones
- Extracción de correlation IDs
- Generación de huellas digitales
- Normalización de valores dinámicos
- Agrupación de incidentes repetidos
- Conteo de apariciones
- Primera y última aparición
- Muestras representativas
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

## Procesar un archivo

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- parse application.log --preview 20
```

## Agrupar incidentes

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- group application.log
```

## Configurar muestras por grupo

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- group application.log --samples 5
```

## Limitar grupos mostrados

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- group application.log --top 10
```

## Excluir entradas sin nivel

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- group application.log --exclude-unknown
```

## Combinar opciones

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- group application.log \
  --samples 3 \
  --top 10 \
  --exclude-unknown
```

## Ejecutar la aplicación de escritorio

```bash
dotnet run \
  --project src/LogLens.Desktop/LogLens.Desktop.csproj
```

## Normalización

LogLens normaliza valores dinámicos como:

- Fechas y horas
- Direcciones IP
- Puertos
- GUID
- Request ID
- Correlation ID
- Trace ID
- Span ID
- URLs
- Valores hexadecimales
- Valores numéricos

Esto permite agrupar mensajes equivalentes aunque cambien sus identificadores, tiempos, direcciones o cantidades.