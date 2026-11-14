# Optarys Management

Repositorio de la App multiplataforma y el Backend de Optarys Management.

El proyecto está en su etapa inicial: contiene la base de Avalonia, una API de ejemplo y las reglas de desarrollo. Las funcionalidades de negocio y la estructura objetivo de Clean Architecture todavía están pendientes de implementación.

## Tecnologías actuales

| Área | Base actual |
| --- | --- |
| App | .NET 10, Avalonia y CommunityToolkit.Mvvm |
| Plataformas de la App | Desktop, Android e iOS |
| Backend | .NET 10 y ASP.NET Core Minimal APIs |
| Compilación anticipada | `PublishAot` habilitado en Desktop y API |

La arquitectura del Backend es independiente del stack. La implementación .NET existente no obliga a mantener esa tecnología; cualquier cambio debe documentarse y cumplir los requisitos acordados, incluido AOT.

## Estructura actual

```text
Optarys Management.slnx
Optarys Management.slnLaunch.user
src/
  Front/
    Core/
      Optarys.Management.App.Core/
      Optarys.Management.App.Infrastructure/
    UI/
      Optarys.Management.UI/
      Optarys.Management.Desktop/
      Optarys.Management.Android/
      Optarys.Management.iOS/
    Directory.Packages.props
  Backend/
    Optarys.Management.API/
```

Las versiones de los paquetes de la App se administran en `src/Front/Directory.Packages.props`.

## Ejecutar en desarrollo

### Requisitos

- SDK de .NET 10 para la implementación actual.
- Acceso a las fuentes NuGet configuradas para restaurar dependencias.
- Para usar la solución y los perfiles compartidos, un Visual Studio compatible con .NET 10, `.slnx` y perfiles de inicio de solución.
- Para Android e iOS, las herramientas, SDKs y workloads correspondientes a la plataforma. No son necesarios para ejecutar únicamente Desktop o API mediante los comandos siguientes.
- Para publicar con AOT, las herramientas nativas requeridas por el sistema y destino de publicación.

### Perfiles de inicio

Abrir `Optarys Management.slnx` y seleccionar el perfil deseado en Visual Studio:

| Perfil | Proyecto que inicia |
| --- | --- |
| `App Desktop` | `Optarys.Management.Desktop` |
| `Api Backend` | `Optarys.Management.API` |

Estos perfiles están incluidos en `Optarys Management.slnLaunch.user`, que se versiona de forma explícita. Cada perfil inicia un proyecto; para ejecutar App y API simultáneamente se pueden usar dos terminales.

### Desde la terminal

Ejecutar desde la raíz del repositorio. `dotnet run` restaura las dependencias necesarias antes de compilar.

**App Desktop:**

```shell
dotnet run --project src/Front/UI/Optarys.Management.Desktop/Optarys.Management.Desktop.csproj
```

**Backend:**

```shell
dotnet run --project src/Backend/Optarys.Management.API/Optarys.Management.API.csproj --launch-profile http
```

Con el perfil `http`, la API usa `http://localhost:5109` en desarrollo. La plantilla actual expone:

- `GET /todos`: datos de ejemplo en memoria.
- `GET /todos/{id}`: consulta de un elemento de ejemplo.
- `GET /openapi/v1.json`: documento OpenAPI en el entorno Development.

Estos endpoints son de la plantilla; no representan funcionalidades de negocio terminadas ni persistencia real.

## Arquitectura acordada

### App

Clean Architecture, organización por funcionalidades y MVVM pragmático:

- Core contiene modelos, operaciones y contratos, sin dependencias de UI o infraestructura.
- Infrastructure implementa acceso a datos e integraciones.
- UI contiene vistas y ViewModels, con composición explícita de dependencias.
- Los hosts Desktop, Android e iOS contienen el arranque y la integración de plataforma.

Dentro de Core, una funcionalidad sigue esta forma:

```text
Features/
  Customers/
    CustomerActions.cs
    Entities/
      Customer.cs
    Dtos/
      CreateCustomerInput.cs
      CustomerSummary.cs
```

`CustomerActions` agrupa operaciones como crear, actualizar y listar. Las entidades y DTOs permanecen en sus carpetas. Las abstracciones pequeñas pueden agruparse con el servicio o en archivos de contratos según su alcance.

### Backend

La estructura objetivo separa Clean Architecture en proyectos o paquetes:

| Proyecto | Responsabilidad | Dependencias internas |
| --- | --- | --- |
| Domain | Entidades y reglas de negocio | Ninguna |
| Application | Actions, DTOs y contratos | Domain |
| Infrastructure | Persistencia e integraciones | Application y Domain |
| API | HTTP, autenticación y composición | Application; Infrastructure solo para composición |

Cada proyecto se organiza por funcionalidades. Las operaciones se agrupan en servicios `Actions`; las entidades y DTOs van en `Entities/` y `Dtos/` de la funcionalidad correspondiente.

Esta separación física aún no está implementada: actualmente existe el proyecto `Optarys.Management.API`. La estructura objetivo puede desplegarse como una sola unidad y no exige microservicios.

`Customers` es un ejemplo de organización, no una funcionalidad implementada.

### AOT

Toda implementación debe considerar AOT y trimming desde el diseño, incluyendo dependencias, serialización y registro de servicios. Habilitar `PublishAot` no acredita por sí solo la compatibilidad: hay que publicar y ejecutar el artefacto para el destino elegido.

Los comandos de desarrollo anteriores no verifican la publicación AOT.

## Reglas de desarrollo

Consultar antes de implementar cambios:

- [Convenciones generales y commits](AGENTS.MD).
- [Historial de cambios](CHANGELOG.md), actualizado únicamente por solicitud explícita.
- [Arquitectura y reglas de la App](src/Front/AGENTS.MD).
- [Arquitectura y reglas del Backend](src/Backend/AGENTS.MD).

La documentación se escribe en español; los identificadores de código y mensajes de commit, en inglés. Los commits siguen este formato:

```text
type(scope): imperative description
```

Ejemplos:

```text
feat(app-core): add customer actions
fix(api): validate customer input
docs(repo): add project readme
```

Verificar los proyectos y comportamientos afectados por cada cambio. No incluir secretos, archivos de compilación ni configuración personal; el archivo de perfiles de inicio de solución es una excepción explícita del `.gitignore`.
