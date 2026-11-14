# Arquitectura de la App

## Decisión inicial

La App combina Clean Architecture, Screaming Architecture y MVVM pragmático:

- **Clean Architecture:** las dependencias apuntan hacia el negocio y sus casos de uso.
- **Screaming Architecture:** las carpetas expresan funcionalidades del producto antes que categorías técnicas.
- **MVVM pragmático:** las vistas presentan, los ViewModels coordinan la interacción y los casos de uso ejecutan comportamiento de la aplicación.

Estas reglas son el objetivo para código nuevo y modificaciones relevantes. La plantilla actual todavía contiene carpetas globales `Views` y `ViewModels`, namespaces `Optarys` y proyectos sin referencias entre capas. Migrar esas piezas al implementar las primeras funcionalidades; no considerar que la arquitectura ya está implementada por existir este documento.

## Límites y dependencias

| Proyecto | Responsabilidad | Referencias permitidas a proyectos de la App |
| --- | --- | --- |
| `Optarys.Management.App.Core` | Modelos, reglas de negocio, acciones y contratos | Ninguna |
| `Optarys.Management.App.Infrastructure` | HTTP, persistencia, almacenamiento y adaptadores externos | Core |
| `Optarys.Management.UI` | Vistas, ViewModels, navegación y composición compartida | Core; Infrastructure exclusivamente para composición |
| Desktop, Android, iOS | Arranque e integración específica de plataforma | UI; Core e Infrastructure cuando la composición lo requiera |

- Dentro de Core, las acciones utilizan los modelos y sus reglas; los modelos no dependen de las acciones que los utilizan.
- Core no depende de Avalonia, CommunityToolkit.Mvvm, HTTP, bases de datos ni tipos de plataforma.
- Infrastructure implementa puertos definidos por quien los consume en Core. Core no conoce sus implementaciones.
- Views y ViewModels no consumen implementaciones ni DTOs de Infrastructure. La referencia de UI a Infrastructure solo permite registrar adaptadores en `Composition/`.
- Los hosts de plataforma no contienen reglas de negocio.
- La App no referencia el proyecto de la API. Sus contratos de transporte se traducen en Infrastructure a modelos internos.
- Core agrupa modelos, reglas y acciones directamente por funcionalidad. No crear carpetas ni namespaces `Domain`, `Application` o `Ports` dentro de cada funcionalidad. Clean Architecture se mantiene mediante las responsabilidades y la dirección de las dependencias.

## Organización por funcionalidad

Ejemplo ilustrativo con `Customers`; no crear esa funcionalidad hasta que exista un requisito:

```text
src/Front/
  Core/
    Optarys.Management.App.Core/
      Features/
        Customers/
          CustomerActions.cs
          Entities/
            Customer.cs
          Dtos/
            CreateCustomerInput.cs
            UpdateCustomerInput.cs
            CustomerSummary.cs
      Shared/
    Optarys.Management.App.Infrastructure/
      Features/
        Customers/
          CustomerApiRepository.cs
          Dtos/
            CustomerApiResponse.cs
      Composition/
  UI/
    Optarys.Management.UI/
      Features/
        Customers/
          CustomerListView.axaml
          CustomerListView.axaml.cs
          CustomerListViewModel.cs
      Shell/
      Shared/
        Controls/
        Presentation/
      Composition/
    Optarys.Management.Desktop/
    Optarys.Management.Android/
    Optarys.Management.iOS/
```

- Mantener nombres de funcionalidades consistentes entre capas. No crear carpetas vacías para completar el esquema.
- Agrupar las operaciones relacionadas de la funcionalidad en un servicio con sufijo `Actions`, directamente dentro de su carpeta. Por ejemplo, `CustomerActions.cs` contiene la clase `CustomerActions` con métodos `CreateAsync`, `UpdateAsync` y `ListAsync`. No crear una clase, archivo o subcarpeta por operación por defecto.
- Guardar las entidades en `Entities/` y los DTOs en `Dtos/`, dentro de la misma funcionalidad. Las entradas y salidas de datos de las operaciones, como `CreateCustomerInput` o `CustomerSummary`, pertenecen a `Dtos/`. No exigir tipos de entrada o resultado para operaciones que no los necesitan.
- Las entidades y los DTOs se mantienen fuera de `CustomerActions.cs`, aunque sean pequeños. Los DTOs breves y relacionados pueden agruparse en un archivo dentro de `Dtos/` cuando mejore la claridad.
- El archivo `CustomerActions.cs` puede contener abstracciones pequeñas relacionadas con el servicio, como `ICustomerRepository`. Si esas abstracciones se comparten entre varios servicios o la agrupación deja de ser clara, utilizar un archivo barril como `CustomerContracts.cs` dentro de la funcionalidad; mantener los DTOs en `Dtos/`.
- Los DTOs de Core describen datos de las operaciones de la App. Los DTOs específicos de HTTP o persistencia pertenecen a `Dtos/` de la misma funcionalidad en Infrastructure y se traducen allí a los tipos de Core. No introducir dependencias de transporte en Core ni crear duplicados sin necesidad.
- Mantener en cada servicio operaciones de una misma responsabilidad de negocio. Separar servicios por responsabilidades concretas solo cuando el crecimiento lo justifique; conservar el sufijo `Actions` y evitar fragmentar por operación de forma automática.
- Mantener vista y ViewModel cercanos dentro de su funcionalidad; subdividir localmente si el tamaño lo exige.
- `Shell` contiene la ventana principal y la estructura de navegación, no funcionalidades de negocio.
- `Shared` solo recibe elementos con reutilización real y una responsabilidad clara. No debe convertirse en un contenedor de código sin dueño.
- Evitar dependencias entre detalles internos de funcionalidades. Cuando deban colaborar, usar contratos explícitos de Core y evitar ciclos.
- Usar el nombre del proyecto como raíz para namespaces nuevos, por ejemplo `Optarys.Management.App.Core.Features.Customers`. Migrar namespaces existentes junto con sus referencias XAML cuando se modifiquen.

## Modelos y acciones

- Los modelos contienen los datos y las reglas de negocio que la App realmente necesita. No duplicar todo el dominio del servidor por simetría; el servidor sigue siendo la autoridad de sus propias reglas.
- Los métodos del servicio `Actions` representan tareas del usuario o del sistema, con entradas, salidas y dependencias explícitas. Cada método es un caso de uso de la App.
- Nombrar el servicio por el concepto de negocio y el sufijo `Actions`, por ejemplo `CustomerActions`; nombrar sus métodos por la operación, como `CreateAsync`, `UpdateAsync` o `ListAsync`.
- El servicio puede ser una clase concreta con dependencias inyectadas por constructor. No exigir una interfaz del servicio, mediador, bus, CQRS ni un repositorio genérico para cada operación.
- Mantener el estado de pantalla en el ViewModel; el servicio `Actions` coordina operaciones y reglas de negocio sin almacenar estado visual.
- Crear puertos específicos según la necesidad: repositorio para acceso a entidades, gateway para un servicio externo. No exponer `HttpResponseMessage`, `DbContext`, `IQueryable` ni DTOs de transporte a Core.
- Los casos de uso no conocen controles, diálogos, navegación visual ni el hilo de UI.
- Representar fallos esperables con resultados explícitos cuando el consumidor necesite distinguirlos; reservar excepciones para fallos inesperados. No crear un framework de resultados antes de necesitarlo.
- Infrastructure traduce errores y contratos de transporte al contrato del puerto; nunca filtra respuestas técnicas o secretos al usuario.

## MVVM pragmático

- Usar CommunityToolkit.Mvvm para propiedades observables y comandos; aprovechar las convenciones existentes compatibles con el proyecto.
- El ViewModel expone estado de presentación y comandos; invoca los métodos del servicio `Actions` y transforma sus resultados para mostrarlos.
- Una interacción exclusivamente visual no necesita un caso de uso. Una operación de negocio o I/O no se implementa directamente en el ViewModel.
- El ViewModel no referencia controles de Avalonia, clientes HTTP ni mecanismos de persistencia.
- El code-behind está permitido para comportamiento visual: foco, animaciones, eventos de controles e interacción específica de plataforma. No contiene reglas de negocio ni acceso a datos.
- No imponer un servicio o una interfaz para cada operación visual. Introducir abstracciones de navegación o diálogos cuando un ViewModel necesite solicitarlos; implementarlas en presentación.
- Mantener `ViewModelBase` mínimo. No agregar dependencias o servicios que todas las pantallas hereden sin necesitarlos.
- Los comandos asíncronos exponen estados relevantes como carga, error y vacío; previenen envíos duplicados y restauran el estado incluso cuando fallan.
- No bloquear con `.Result` o `.Wait()`. Evitar `async void` salvo manejadores de eventos; en esos casos, manejar los errores en el límite del evento.
- Cancelar operaciones cuando su ciclo de vida lo requiera y liberar suscripciones y recursos. No mostrar una cancelación normal como un error.
- Actualizar estado observable en el hilo de UI cuando sea necesario; mantener ese detalle fuera de Core.

## AOT en la App

- Aplicar el requisito general de AOT en Core, Infrastructure, UI y hosts, considerando el mecanismo de publicación de cada plataforma.
- Preferir bindings compilados y tipos de datos explícitos en XAML. Evitar resolver vistas o ViewModels mediante búsqueda de tipos por nombre o escaneo de ensamblados; usar asociaciones o fábricas explícitas.
- Para serialización, preferir contratos conocidos en compilación y generación de código compatible con AOT. Mantener esa configuración en Infrastructure cuando corresponda al transporte o la persistencia.
- Mantener la composición y las integraciones de plataforma compatibles con AOT; verificar especialmente navegación, bindings, serialización y resolución de dependencias en el artefacto publicado cuando esos mecanismos cambien.
- Estas reglas no implican que los proyectos actuales ya tengan configurada o validada la publicación AOT.

## Validación, composición y estado

- La vista y el ViewModel validan formato e interacción para dar respuesta inmediata; los modelos protegen sus reglas de negocio y las acciones comprueban las condiciones necesarias para ejecutarse.
- Las validaciones del cliente no sustituyen las del servidor.
- Crear dependencias en un punto explícito de composición compartida, con extensiones de plataforma en cada host. Usar inyección por constructor; no resolver servicios desde ViewModels con un localizador global.
- Elegir ciclos de vida explícitos: ViewModels de pantalla normalmente por instancia; estado compartido solo cuando el producto lo requiere. No convertir ViewModels en singleton por comodidad.
- No introducir contenedor DI, caché, sincronización offline ni librerías adicionales hasta que exista una necesidad concreta.

## Pruebas y revisión

- Modelos: probar reglas de negocio y cambios de estado.
- Acciones: probar resultados, fallos esperables y coordinación mediante sustitutos de las dependencias externas.
- ViewModels: probar comportamiento relevante de comandos y estado sin levantar controles.
- Infrastructure: usar pruebas de integración en límites cuyo comportamiento dependa del protocolo o almacenamiento.
- No exigir pruebas para propiedades triviales o cambios visuales de bajo impacto. Verificar visualmente los flujos de UI afectados cuando sea posible.
- Al revisar un cambio, comprobar dirección de dependencias, ubicación por funcionalidad, ausencia de I/O en presentación y manejo de fallos/cancelación pertinentes.
- Añadir verificaciones automatizadas de arquitectura cuando existan funcionalidades y una infraestructura de pruebas que justifiquen su mantenimiento.
