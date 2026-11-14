# Arquitectura del Backend

## Decisión inicial

- Aplicar Clean Architecture con separación física en proyectos, paquetes o módulos de dependencias controladas: Domain, Application, Infrastructure y API.
- La arquitectura no depende de .NET, un framework HTTP, un ORM ni una base de datos. Elegir la tecnología según las necesidades del producto, operación y compatibilidad AOT, y registrar la decisión antes de implementar un cambio de stack.
- La API .NET existente es el punto de partida actual, no una obligación tecnológica. Estas reglas no implican que se haya migrado o elegido otro stack.
- Mantener Screaming Architecture dentro de cada proyecto: organizar por funcionalidades y agrupar operaciones relacionadas en servicios `Actions`.
- La separación es de código y dependencias; no exige microservicios ni despliegues independientes. Mantener un Backend desplegable como una unidad mientras no exista una necesidad distinta.
- MVVM pertenece a la App y no se aplica al Backend.

## Proyectos y dependencias

| Proyecto o paquete | Contenido | Dependencias internas permitidas |
| --- | --- | --- |
| Domain | Entidades y reglas propias del negocio | Ninguna |
| Application | Servicios Actions, entradas, salidas y contratos que necesitan las operaciones | Domain |
| Infrastructure | Persistencia e integraciones externas | Application y Domain |
| API | HTTP, autenticación, arranque y composición | Application; Infrastructure solo para composición |

- Domain y Application no dependen del framework web, ORM, serializador ni infraestructura de despliegue.
- Infrastructure implementa los contratos definidos por las operaciones en Application; Application no conoce esas implementaciones.
- API llama a Application y traduce sus resultados a HTTP. Sus endpoints no acceden directamente a repositorios ni a Infrastructure.
- La composición en API conecta implementaciones y contratos. Ese es el único lugar de API que conoce Infrastructure.
- Las entidades no dependen de Actions. Evitar dependencias circulares entre proyectos y entre funcionalidades.
- Expresar estos límites en referencias de proyecto, dependencias de paquetes o reglas de importación verificables, según el lenguaje. No sustituir la separación solicitada por carpetas dentro de un único proyecto.

## Organización por funcionalidad

Ejemplo sin extensiones de archivo para no imponer un lenguaje. Cada elemento sin barra final representa un archivo; los nombres de proyectos se adaptan al stack elegido.

```text
src/Backend/
  Domain/
    Features/
      Customers/
        Entities/
          Customer
  Application/
    Features/
      Customers/
        CustomerActions
        Dtos/
          CreateCustomerInput
          UpdateCustomerInput
          CustomerSummary
  Infrastructure/
    Features/
      Customers/
        CustomerRepository
  API/
    Features/
      Customers/
        CustomerEndpoints
    Composition/
```

- Mantener los mismos nombres de funcionalidades entre proyectos. No repetir carpetas Domain o Application dentro de una feature.
- `CustomerActions` agrupa métodos para crear, actualizar, consultar y listar clientes. No crear una clase o archivo por operación por defecto.
- `CustomerEndpoints` agrupa las rutas HTTP de la funcionalidad; puede adoptar la forma de router o controlador propia del framework elegido.
- Las entidades siempre van en `Entities/` de la funcionalidad en Domain. Los DTOs de las operaciones van en `Dtos/` de la funcionalidad en Application, aunque sean pequeños.
- Los DTOs específicos de HTTP pertenecen a `Dtos/` en API; los de proveedores externos, a `Dtos/` en Infrastructure. Crear estos tipos solo cuando exista una diferencia real que traducir. API puede reutilizar DTOs de Application si son apropiados para el contrato público y no requieren detalles del framework.
- Las abstracciones pequeñas que necesita el servicio pueden vivir junto a `CustomerActions`, si el lenguaje lo permite. Si se comparten entre servicios, agruparlas en un archivo barril como `CustomerContracts` dentro de Application. Las implementaciones permanecen en Infrastructure.
- Agrupar DTOs breves relacionados dentro de un archivo de `Dtos/` cuando mejore la claridad. No moverlos al archivo Actions.
- Crear carpetas y archivos según necesidades reales. `Shared/`, si hace falta dentro de un proyecto, solo contiene elementos con reutilización real y respeta sus dependencias permitidas.
- Separar servicios Actions por responsabilidades de negocio cuando su crecimiento lo justifique; no fragmentar automáticamente cada operación.

## Operaciones y convenciones

- Los servicios Actions coordinan reglas, permisos sobre recursos, transacciones y dependencias externas. Reciben datos explícitos y devuelven resultados independientes de HTTP.
- Las entidades protegen sus reglas y cambios de estado. No incorporarles tipos de persistencia ni configuración del framework.
- No usar contextos HTTP, respuestas HTTP, clientes concretos de proveedores ni contextos de base de datos dentro de Application.
- Usar dependencias explícitas mediante el mecanismo idiomático del lenguaje. No exigir un contenedor DI, interfaz por servicio, repositorio genérico, mediador, CQRS o bus.
- Mantener el sufijo Actions como convención conceptual. Adaptar nombres de archivos, métodos, asincronía e interfaces al lenguaje elegido; las convenciones C# de la raíz se aplican al código C#.
- No compartir entidades, servicios ni referencias de proyecto entre App y Backend. Integrar ambos mediante contratos públicos documentados.
- Mantener los servicios libres de estado mutable de solicitudes; seleccionar ciclos de vida compatibles para sus dependencias.

## HTTP, acceso y validación

- Mantener el punto de entrada centrado en configuración, composición y registro de funcionalidades.
- Agrupar rutas por recurso, por ejemplo `/customers` y `/customers/{id}`, usando verbos HTTP y nombres consistentes.
- Los endpoints reciben datos, obtienen la identidad autenticada, invocan Actions y traducen resultados. No contienen reglas de negocio.
- Validar formato y campos obligatorios en la entrada; comprobar condiciones de la operación en Application e invariantes en Domain. No confiar en validaciones de la App.
- Aplicar autenticación y políticas de acceso en rutas protegidas. Verificar también permisos sobre el recurso en la operación cuando corresponda; no confiar en identificadores de usuario o tenant enviados por el cliente.
- Mapear explícitamente campos de entrada y salida. No exponer entidades de almacenamiento como contratos públicos ni permitir modificar campos internos por asignación indiscriminada.
- Mantener respuestas consistentes: 200 para consultas o resultados, 201 con ubicación para creación, 204 para éxito sin cuerpo, 400 para entradas inválidas, 401 para falta de autenticación, 403 para acceso denegado, 404 para recursos no encontrados y 409 para conflictos de estado cuando corresponda.
- Distinguir fallos de negocio esperables mediante resultados explícitos. Traducirlos a HTTP en API y centralizar errores inesperados con respuestas Problem Details.
- No exponer trazas, secretos ni datos sensibles en respuestas. Registrar información técnica útil con identificadores de correlación.
- Paginar listados potencialmente grandes con límites máximos y orden estable. Validar filtros y ordenamientos admitidos.
- Documentar contratos HTTP y respuestas en OpenAPI, y señalar cambios incompatibles en los commits.

## Persistencia y ejecución

- Mantener consultas y detalles del proveedor en Infrastructure. No filtrar tipos del ORM, consultas diferidas del proveedor o conexiones hacia Application o API.
- Introducir contratos específicos para las necesidades reales de las operaciones.
- Definir transacciones por operación de negocio cuando varias escrituras deban ser atómicas; implementar sus mecanismos en Infrastructure. Proteger integridad y unicidad también en el almacenamiento cuando corresponda.
- Considerar concurrencia e idempotencia cuando existan actualizaciones simultáneas o reintentos. No reintentar escrituras a ciegas.
- Propagar cancelación y plazos de ejecución según el soporte del stack. No bloquear innecesariamente operaciones asíncronas ni lanzar trabajos sin supervisión desde un endpoint.
- Elegir proveedor, librerías y estrategia de migración por requisitos concretos y compatibilidad con el despliegue, sin imponer un ORM.

## AOT y elección tecnológica

- Mantener el requisito general de considerar AOT desde el diseño. La independencia de .NET no elimina este requisito.
- Antes de elegir un stack, documentar cómo soporta la compilación anticipada y cómo se construye y verifica el artefacto para el destino de despliegue. Si no cumple el requisito, plantear explícitamente el cambio de requisito antes de adoptarlo.
- Aplicar las verificaciones de trimming cuando el stack utilice eliminación de código o metadatos. No asumir que todos los lenguajes usan las mismas opciones de publicación.
- Preferir dependencias y registros explícitos. Evitar generación dinámica de código o descubrimiento por reflexión sin soporte verificado para el destino.
- Verificar serialización, autenticación, persistencia y demás integraciones en el artefacto publicado. No suprimir advertencias para ocultar incompatibilidades.
- En la implementación .NET actual, conservar `PublishAot`, el arranque con `CreateSlimBuilder` y la serialización mediante contextos generados mientras ese stack siga vigente. Registrar los tipos de entrada y salida necesarios.
- Verificar compilación/publicación AOT y ejecución para el destino cuando cambien mecanismos relevantes. Informar comprobaciones pendientes si faltan herramientas; una compilación normal no acredita compatibilidad AOT.

## Pruebas y entrega

- Domain: probar reglas y cambios de estado.
- Application: probar resultados, permisos y coordinación mediante sustitutos de contratos externos, sin levantar HTTP ni una base de datos.
- Infrastructure: probar consultas, restricciones, transacciones e integraciones con un entorno representativo.
- API: probar contratos HTTP, validación, acceso y traducción de resultados.
- Verificar las dependencias permitidas entre proyectos con herramientas adecuadas al stack. Probar también el artefacto AOT en los flujos afectados.
- No escribir pruebas para DTOs triviales ni para cambios solo documentales.
- Usar el scope `api` para cambios del Backend mientras no se acuerden scopes más específicos; no depende del lenguaje elegido.
- El repositorio conserva por ahora la plantilla .NET existente. La separación física y cualquier cambio de tecnología son trabajo de implementación pendiente; estas reglas documentan el objetivo.
