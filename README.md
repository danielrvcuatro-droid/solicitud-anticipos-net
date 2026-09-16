# Solicitud de Anticipos (.NET)

Reemplazo del flujo de Power Automate **"Automate Solicitud de Anticipos"** de RVCUATRO por un
sistema propio en .NET 8. Microsoft Forms se mantiene como punto de captura (un flujo mínimo de
Power Automate de un solo paso reenvía la respuesta a nuestra API); toda la lógica de negocio,
orquestación, almacenamiento de datos y notificaciones vive aquí.

## Por qué se migra

El flujo original en Power Automate funciona, pero:

- La lógica de aprobación (matrices por departamento, urgencia, analista financiero condicional)
  está repartida en decenas de acciones y condiciones anidadas, difícil de mantener o probar.
- No hay trazabilidad completa: solo queda rastro en el PDF final, no un log de cada evento.
- Las matrices de aprobadores están "quemadas" en el flujo; cambiarlas requiere editar Automate.
- La aprobación es siempre secuencial (un aprobador a la vez), aunque varios podrían aprobar en paralelo.

## Qué se mantiene y qué mejora

| Aspecto | Flujo original (Power Automate) | Este sistema (.NET) |
|---|---|---|
| Captura de la solicitud | Microsoft Forms | Microsoft Forms (sin cambios para el usuario) |
| Backend / orquestación | Power Automate | ASP.NET Core API + Worker Service propios |
| Datos y bitácora | SharePoint (lista) | Supabase (Postgres) |
| Adjuntos y PDF de seguimiento | SharePoint | SharePoint (sin cambios, vía Microsoft Graph) |
| Aprobación | Correo → "My Approvals" (Teams/Power Automate) | Correo → panel web propio, autenticación por *magic link* |
| Matrices de aprobadores | Fijas dentro del flujo | Configurables (base de datos / panel de administración) |
| Modo de aprobación | Siempre secuencial | Secuencial o paralelo, configurable por nivel |
| Auditoría | Parcial (solo el PDF final) | Bitácora completa de cada evento |

## Arquitectura

```
SolicitudAnticipos/
├── src/
│   ├── SolicitudAnticipos.Domain/          → entidades, reglas de negocio, máquina de estados (sin dependencias externas)
│   ├── SolicitudAnticipos.Infrastructure/  → Supabase (EF Core/Npgsql), Microsoft Graph (SharePoint, correo), generación de PDF
│   ├── SolicitudAnticipos.Api/             → recibe la respuesta de Forms, expone endpoints para el panel web
│   ├── SolicitudAnticipos.Web/             → panel de aprobadores (login por magic link, bandeja de aprobaciones)
│   └── SolicitudAnticipos.Worker/          → revisa vencimientos (timeouts) y envía recordatorios
└── tests/
    └── SolicitudAnticipos.Domain.Tests/    → pruebas unitarias de la máquina de estados
```

### Flujo de una solicitud

1. El colaborador llena el formulario de Microsoft Forms (sin cambios).
2. Un flujo mínimo de Power Automate (solo trigger + una llamada HTTP) reenvía la respuesta cruda a
   `SolicitudAnticipos.Api`.
3. La API resuelve la matriz de aprobación aplicable (departamento/sociedad), determina si es urgente
   y si debe incluirse un analista financiero, y crea la `Solicitud` con su cadena de aprobación.
4. Se notifica por correo al primer aprobador (o a todo el nivel, si es paralelo) con un enlace de
   un solo uso al panel web.
5. `SolicitudAnticipos.Worker` revisa periódicamente los pasos vencidos y dispara los timeouts.
6. Al completarse (aprobada, rechazada o vencida), se genera/actualiza el PDF de seguimiento en
   SharePoint y se notifica el resultado.

### Máquina de estados (dominio)

- **Solicitud**: `Pendiente → EnAprobacion → {Aprobada | Rechazada | Vencida}`
- **PasoAprobacion**: `Programado → Pendiente → {Aprobado | Rechazado | Vencido}` (o `Omitido` si la
  solicitud ya se resolvió antes de llegar a ese paso)

Toda esta lógica vive en `SolicitudAnticipos.Domain` sin dependencias de base de datos, correo ni
HTTP — se puede probar de forma aislada (ver `tests/SolicitudAnticipos.Domain.Tests`).

## Cómo correr el proyecto

Requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Compilar toda la solución
dotnet build

# Correr las pruebas del dominio
dotnet test tests/SolicitudAnticipos.Domain.Tests
```

Los proyectos `Api`, `Web` y `Worker` todavía no tienen su configuración de Supabase/Graph/SMTP
(eso llega en los siguientes pasos del roadmap); por ahora el foco es la capa de dominio.

## Roadmap

- [x] Modelo de dominio y máquina de estados (`Solicitud`, `PasoAprobacion`) con pruebas unitarias
- [ ] Persistencia en Supabase (EF Core + Npgsql) — repositorios de `Solicitud` y `MatrizAprobacion`
- [ ] Endpoint de ingesta en la API (`POST /api/solicitudes/ingest`) + flujo mínimo de Power Automate
- [ ] Integración con Microsoft Graph: subir adjuntos/PDF a SharePoint, enviar correos
- [ ] Generación del PDF de seguimiento (QuestPDF)
- [ ] Panel web de aprobadores con autenticación por magic link
- [ ] Worker: revisión de vencimientos y recordatorios
- [ ] Panel de administración de matrices de aprobación

## Convenciones de este repositorio

- **Commits**: [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `test:`,
  `docs:`, `refactor:`, `chore:`).
- **Ramas**: `main` siempre estable; una rama `feature/<nombre>` por cada punto del roadmap, integrada
  a `main` vía Pull Request.
- **CI**: cada push y PR corre `dotnet build` + `dotnet test` (ver `.github/workflows/ci.yml`).
