# LogLens

Local log analysis and incident diagnostics for developers.

LogLens es una aplicación multiplataforma para leer, estructurar, agrupar, diagnosticar y explorar archivos de logs localmente.

## Estado

Versión actual: 0.8.0

## Funciones

- Lectura progresiva mediante streaming
- Procesamiento sin cargar el archivo completo en memoria
- Progreso por líneas, bytes y porcentaje
- Cancelación
- Parser de texto
- Parser de JSON Lines
- Detección de timestamps
- Detección de niveles
- Extracción de servicios
- Extracción de excepciones
- Extracción de códigos HTTP
- Extracción de duraciones
- Generación de huellas
- Agrupación de incidentes repetidos
- Diagnóstico local mediante reglas
- Priorización
- Evidencia
- Acciones recomendadas
- Interfaz gráfica con Avalonia
- Selector de archivos
- Arrastrar y soltar
- Búsqueda general
- Filtro por nivel
- Filtro por prioridad
- Ordenamiento de grupos
- Ordenamiento de diagnósticos
- Selección de muestras
- Copia al portapapeles
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

## Ejecutar la aplicación

```bash
dotnet run \
  --project src/LogLens.Desktop/LogLens.Desktop.csproj
```

## Exploración

Después de analizar un archivo puedes buscar por:

- Mensaje
- Servicio
- Excepción
- Código HTTP
- Huella digital
- Mensaje de muestra
- Título del diagnóstico
- Resumen
- Regla
- Evidencia
- Acción recomendada

## Filtros

Los incidentes pueden filtrarse por nivel:

- Critical
- Error
- Warning
- Information
- Debug
- Trace
- Unknown

Los diagnósticos pueden filtrarse por prioridad:

- Crítica
- Alta
- Media
- Baja
- Sin prioridad

## Ordenamiento

Los incidentes pueden ordenarse por:

- Gravedad
- Frecuencia
- Más recientes
- Más antiguos
- Mensaje

Los diagnósticos pueden ordenarse por:

- Prioridad
- Confianza
- Más recientes
- Título

## Copiar información

La interfaz permite copiar:

- Mensaje del incidente
- Huella del incidente
- Detalles completos
- Muestra original
- Resumen del diagnóstico
- Huella del diagnóstico
- Acciones recomendadas
- Diagnóstico completo

## CLI

```bash
dotnet run \
  --project src/LogLens.Cli/LogLens.Cli.csproj \
  -- diagnose application.log
```

## Privacidad

El análisis se ejecuta localmente. Los archivos no necesitan enviarse a servicios externos.