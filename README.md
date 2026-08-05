# LogLens

Local log analysis and incident diagnostics for developers.

LogLens es una aplicación multiplataforma para leer, estructurar, agrupar y diagnosticar archivos de logs localmente.

## Estado

Versión actual: 0.7.0

## Funciones principales

- Lectura progresiva mediante streaming
- Procesamiento sin cargar el archivo completo en memoria
- Progreso por líneas, bytes y porcentaje
- Cancelación del análisis
- Parser de texto genérico
- Parser de JSON Lines
- Detección de timestamps
- Detección de niveles
- Extracción de servicios
- Extracción de excepciones
- Extracción de códigos HTTP
- Extracción de duraciones
- Extracción de correlation IDs
- Generación de huellas digitales
- Normalización de valores dinámicos
- Agrupación de incidentes repetidos
- Muestras representativas
- Diagnóstico local mediante reglas
- Priorización de incidentes
- Evidencia de diagnóstico
- Acciones recomendadas
- Detección de errores críticos
- Detección de fallos recurrentes
- Detección de errores HTTP
- Detección de latencia elevada
- Detección de fallos de conexión
- Interfaz gráfica con Avalonia
- Selector de archivos
- Arrastrar y soltar
- Resumen visual
- Exploración de grupos
- Exploración de diagnósticos
- CLI
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

## Ejecutar la aplicación de escritorio

```bash
dotnet run \
  --project src/LogLens.Desktop/LogLens.Desktop.csproj
```

Desde la aplicación puedes:

1. Presionar `Seleccionar archivo`.
2. Elegir un archivo compatible.
3. Presionar `Analizar`.
4. Revisar los incidentes agrupados.
5. Revisar los diagnósticos.
6. Consultar evidencias y acciones recomendadas.
7. Cancelar el análisis cuando sea necesario.

También puedes arrastrar un archivo desde el administrador de archivos hasta la ventana.

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
  -- read application.log \
  --preview 20
```

## Procesar un archivo

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- parse application.log \
  --preview 20
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

## Filtrar diagnósticos por prioridad

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

## Reglas incluidas

LogLens incluye reglas locales para detectar:

- Incidentes con nivel crítico
- Fallos recurrentes
- Respuestas HTTP 4xx
- Respuestas HTTP 5xx
- Límites HTTP 429
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

## Privacidad

El análisis se ejecuta localmente. Los archivos no necesitan enviarse a servicios externos.