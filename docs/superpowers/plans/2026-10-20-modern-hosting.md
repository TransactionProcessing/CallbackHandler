# Modern Hosting Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate CallbackHandler to ASP.NET Core minimal hosting while preserving existing host, service, middleware, and endpoint behavior.

**Architecture:** `Program.cs` becomes the composition root for `WebApplicationBuilder` and `WebApplication`. Lamar registries receive configuration explicitly; the legacy `Startup` class and its static configuration handoff are removed after production and test callers migrate.

**Tech Stack:** .NET 10, ASP.NET Core minimal hosting, Lamar, NLog, Sentry, existing Shared and TransactionProcessing.ApiDefaults packages.

**Spec:** `docs/superpowers/specs/2026-10-20-modern-hosting-design.md`

## Global Constraints

- Preserve current configuration file sources and their ordering.
- Preserve Windows Service, Lamar, Kestrel, NLog, Sentry conditions/options, middleware, and endpoint paths/response writers.
- Do not upgrade unrelated dependencies.
- Do not add or run tests unless requested by the user; perform the requested review/build verification only when authorized.

## Review Focus

- Missing optional configuration files must remain optional and environment-specific configuration must still load.
- Sentry must remain disabled in Development and enabled only when the Sentry configuration section exists.
- Configuration must be available before EventStore and health monitoring registries are constructed.
- Middleware must run before mapped endpoint handling, and callback and health routes must retain their current paths and writers.
- `UseApiDefaults` must be applied to the built `WebApplication` so version discovery and OpenAPI/Scalar routes are registered.

---

### Task 1: Modernize the host composition root

**Files:**
- Modify: `CallbackHandler/Program.cs`
- Test/update: `CallbackHandler.Tests/BootstrapperTests.cs` as required by changed composition API

**Interfaces:**
- Produces: a `WebApplication` host configured with the existing sources, host integrations, services, and application pipeline.

- [x] Create the modern builder with the existing args and configure hosting, configuration sources, logging, Windows Service, Kestrel, Lamar, and Sentry from the design spec.
- [x] Move the registrations currently supplied by `Startup.ConfigureContainer` into Lamar's supported modern builder configuration.
- [x] Build the app, initialize logger/configuration, preserve Development exception behavior and middleware, map callback and health routes, invoke `UseApiDefaults`, then run.
- [x] Remove `UseStartup<Startup>()` and obsolete host-builder helpers after their responsibilities move.

### Task 2: Remove static Startup configuration from registries

**Files:**
- Modify: `CallbackHandler/Bootstrapper/MiddlewareRegistry.cs`
- Modify: `CallbackHandler/Bootstrapper/RepositoryRegistry.cs`
- Modify: `CallbackHandler/Startup.cs` (remove after remaining references are migrated)
- Modify: `CallbackHandler.Tests/BootstrapperTests.cs`
- Modify: `CallbackHandler.BusinessLogic.Tests/Mediator/MediatorTests.cs`

**Interfaces:**
- Consumes: the modern Lamar composition established in Task 1.
- Produces: registrations initialized with configuration supplied through the new composition path, with no production registry reads from `Startup.Configuration`.

- [x] Update registry construction/configuration to use the explicit configuration path supported by the installed Lamar integration.
- [x] Update tests and any remaining production references to the removed Startup configuration/container members.
- [x] Delete `Startup.cs` when no callers remain.

### Task 3: Verify migration behavior

**Files:**
- No planned source changes unless verification exposes a defect.

**Interfaces:**
- Consumes: completed host and registry migration.

- [x] Build `CallbackHandler/CallbackHandler.csproj` and inspect diagnostics.
- [ ] Run relevant bootstrapper and integration tests if authorized; verify callback, health, OpenAPI, and Scalar routes and preserved Sentry/configuration behavior.
- [ ] Review the final diff for endpoint ordering, retained configuration sources, and stale `Startup` references.
