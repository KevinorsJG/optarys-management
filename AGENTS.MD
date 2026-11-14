# Reglas del repositorio

## Alcance

- Este archivo define las convenciones generales del repositorio.
- `src/Front/AGENTS.MD` define la arquitectura de la App. No aplicar esas decisiones automáticamente al Backend.
- `src/Backend/AGENTS.MD` define la arquitectura del Backend.
- Las instrucciones más específicas complementan estas reglas en su directorio.
- Documentación y comunicación del proyecto en español; identificadores de código y mensajes de commit en inglés.

## Convenciones de código

- Usar nombres que expresen intención y vocabulario del negocio. Evitar `Manager`, `Helper` y `Utils` como contenedores genéricos.
- C#: `PascalCase` para tipos, métodos y propiedades; `camelCase` para parámetros y variables; `_camelCase` para campos privados; prefijo `I` para interfaces.
- Usar namespaces de ámbito de archivo. Alinear los namespaces nuevos con el proyecto y la ruta de carpetas.
- Mantener nullable habilitado. Expresar la ausencia con tipos anulables; no ocultarla con `!` sin una justificación concreta.
- Métodos asíncronos con sufijo `Async`. Propagar `CancellationToken` en operaciones de I/O cuando corresponda.
- Preferir composición e inyección por constructor. Introducir interfaces en límites externos o cuando exista una necesidad real de sustitución, no para cada clase.
- Mantener cambios enfocados. Evitar abstracciones, paquetes y reorganizaciones que no resuelvan una necesidad actual.
- No incluir secretos, credenciales, datos personales, artefactos de compilación ni configuración local del desarrollador en el control de versiones.
- Documentar el motivo de decisiones no evidentes; evitar comentarios que repitan el código.

## Compatibilidad AOT

- Toda implementación debe considerar desde el diseño la compilación anticipada (AOT) y el trimming, incluidos sus paquetes y mecanismos de integración.
- Preferir referencias estáticas, registros explícitos y generación de código en compilación. Evitar depender de generación dinámica de código, descubrimiento de tipos por reflexión o carga dinámica de ensamblados.
- Cuando sea necesaria reflexión, limitarla y declarar los requisitos de preservación correspondientes; verificar su funcionamiento en el artefacto publicado para el destino afectado.
- Evaluar la compatibilidad AOT y trimming antes de incorporar o actualizar dependencias. No suprimir advertencias sin justificar y verificar la solución.
- Validar los cambios que afecten estos mecanismos mediante publicación y ejecución para el destino AOT correspondiente, cuando el entorno lo permita. Una compilación normal no acredita compatibilidad AOT; indicar cualquier validación pendiente.

## Abstracciones pequeñas y archivos

- Evitar crear archivos separados para abstracciones muy cortas y estrechamente relacionadas. No imponer un archivo por tipo.
- Si una abstracción solo sirve a una implementación y ambas pertenecen a la misma capa y ámbito, declararla en el mismo archivo, con la visibilidad mínima necesaria.
- Si varios consumidores del mismo ámbito comparten abstracciones pequeñas, agruparlas en un archivo "barril" con nombre específico, por ejemplo `CustomerContracts.cs`. En este repositorio, "barril" significa un archivo C# que reúne declaraciones relacionadas.
- Preservar los límites de arquitectura: un puerto de Core permanece en Core aunque su implementación esté en Infrastructure. La agrupación nunca debe invertir dependencias ni ampliar artificialmente la visibilidad.
- Separar una abstracción cuando crezca, tenga responsabilidad propia o la agrupación dificulte encontrarla y mantenerla. No agrupar tipos sin relación en archivos globales como `Abstractions.cs`.

## Commits

Adoptar el siguiente formato, compatible con Conventional Commits:

```text
type(scope): imperative description
```

- Tipos: `feat` (capacidad nueva), `fix` (corrección), `refactor` (reestructuración sin cambiar comportamiento), `perf` (rendimiento), `test`, `docs`, `build` (compilación o dependencias), `ci`, `chore` (mantenimiento) y `revert`.
- Scope obligatorio en este repositorio. Inicialmente: `app`, `app-core`, `app-infrastructure`, `app-ui`, `android`, `ios`, `desktop`, `api` y `repo`. Usar `repo` para cambios transversales.
- Descripción en inglés, imperativa, sin punto final. Procurar que la primera línea no exceda 72 caracteres.
- Un commit debe representar un cambio lógico revisable. Incluir las pruebas y documentación necesarias para ese cambio.
- Usar un cuerpo cuando sea necesario explicar el motivo, impacto o una migración.
- Para incompatibilidades de contratos públicos, usar `!` antes de `:` y un pie `BREAKING CHANGE:` con el impacto y la acción de migración.
- No usar mensajes como `update`, `fix stuff` o `WIP` en el historial integrado.

Ejemplos:

```text
feat(app-core): add customer registration use case
fix(app-ui): prevent duplicate form submission
refactor(app-infrastructure): isolate customer API mapping
docs(repo): define application architecture rules
feat(api)!: change customer identifier format

BREAKING CHANGE: Clients must send UUID customer identifiers.
```

## Verificación y entrega

- Compilar los proyectos afectados cuando cambie código y ejecutar las pruebas pertinentes disponibles.
- Probar reglas de negocio y comportamiento observable; no escribir pruebas que solo reproduzcan la implementación.
- Indicar qué se verificó y cualquier verificación que no haya podido ejecutarse.
- Los cambios solo documentales requieren revisar consistencia, rutas y ejemplos; no requieren compilar la solución.
- Registrar decisiones estructurales relevantes en `docs/decisions/NNNN-short-title.md`, con contexto, decisión y consecuencias. No crear un ADR para cada cambio rutinario.
- No crear commits ni publicar cambios sin una solicitud del usuario.
