# LogLens

Local log analysis and incident diagnostics for developers.

LogLens es una aplicación multiplataforma para leer, procesar, estructurar, agrupar y diagnosticar archivos de logs localmente.

## Estado

Versión actual: 0.6.0

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
- Diagnóstico local mediante reglas
- Priorización de incidentes
- Evidencia de diagnóstico
- Acciones recomendadas
- Detección de niveles críticos
- Detección de fallos recurrentes
- Detección de errores HTTP
- Detección de latencia elevada
- Detección de fallos de conexión
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
  -- read application.log --preview 20
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
  -- group application.log \
  --samples 3 \
  --top 10
```

## Diagnosticar incidentes

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- diagnose application.log
```

## Configurar muestras

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- diagnose application.log \
  --samples 5
```

## Limitar diagnósticos

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- diagnose application.log \
  --top 10
```

## Filtrar por prioridad

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- diagnose application.log \
  --min-priority high
```

Prioridades disponibles:

- `none`
- `low`
- `medium`
- `high`
- `critical`

## Excluir entradas sin nivel

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- diagnose application.log \
  --exclude-unknown
```

## Combinar opciones

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- diagnose application.log \
  --samples 3 \
  --top 10 \
  --exclude-unknown \
  --min-priority medium
```

## Ejecutar la aplicación de escritorio

```bash
dotnet run \
  --project src/LogLens.Desktop/LogLens.Desktop.csproj
```

## Reglas de diagnóstico

LogLens incluye reglas locales para detectar:

- Incidentes con nivel crítico
- Fallos recurrentes
- Respuestas HTTP 4xx
- Respuestas HTTP 5xx
- Límites de solicitudes HTTP 429
- Latencia elevada
- Tiempos de espera
- Fallos de conexión
- Errores de red
- Excepciones de socket

## Códigos de salida de la CLI

- `0`: operación completada
- `1`: error de argumentos o ejecución
- `2`: existen líneas no reconocidas
- `3`: se detectaron incidentes críticos
- `130`: operación cancelada

## Procesamiento local

El análisis se ejecuta localmente. Los archivos de logs no necesitan enviarse a servicios externos.