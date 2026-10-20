# Modern Hosting Migration Design

## Goal

Move CallbackHandler from the `IHostBuilder` plus `Startup` model to ASP.NET Core minimal hosting so WebApplication-only endpoint extensions such as `UseApiDefaults` can be used directly.

## Agreed scope

- Replace `Host.CreateDefaultBuilder` and `UseStartup<Startup>()` with `WebApplication.CreateBuilder` in `Program.cs`.
- Preserve current hosting configuration sources, Windows service integration, Kestrel, logging, and conditional Sentry configuration.
- Move current service registrations and HTTP pipeline behavior into the modern builder and `WebApplication` pipeline.
- Keep Lamar and its service registries, while eliminating production service-registration dependencies on `Startup.Configuration`.
- Register callback, health check, OpenAPI, and Scalar endpoints in the modern pipeline. Use `UseApiDefaults` on the built `WebApplication`.
- Update tests and other call sites that depend on `Startup.ConfigureContainer` or `Startup.Configuration`.

## Existing behavior to preserve

- Load `hosting.json`, environment-specific hosting configuration, the `/home/txnproc/config/appsettings*.json` files, local `appsettings*.json`, and environment variables in the existing order.
- Set `HealthMonitoring:Service:Version` from the entry assembly version, falling back to `0.0.0.0`.
- Use Windows Service integration, Lamar, NLog plus console logging, and Kestrel.
- Enable Sentry only when its configuration section exists and the environment is not Development; preserve its existing options.
- In Development, use the developer exception page.
- Initialize logger and `ConfigurationReader`; preserve tenant, request/response logging, and exception middleware behavior.
- Map callback endpoints and both existing health check routes with their current response writers.
- Preserve service registries and the existing test composition behavior where practical.

## Design

`Program.cs` becomes the composition root. It creates the `WebApplicationBuilder`, applies the app's existing configuration and host settings, configures Lamar registries, builds the app, initializes runtime logging/configuration, adds middleware, maps callback and health endpoints, calls `UseApiDefaults`, and runs the app.

Configuration-dependent registry constructors will receive configuration explicitly rather than reading mutable static state from `Startup`. The registry inclusion mechanism will follow the supported Lamar integration API available in the repository's package versions. `Startup.cs` will be removed once its responsibilities and references have moved. Test fixtures will compose registrations through the replacement API.

Endpoint registration will retain routing semantics: middleware is installed before endpoint dispatch, and callback, health, OpenAPI, and Scalar routes are all mapped on the built application. The migration will avoid changing endpoint paths or response behavior.

## Validation

Build the CallbackHandler project and run its existing relevant tests, adjusting tests only where startup composition APIs changed. Confirm the OpenAPI and Scalar routes are registered by `UseApiDefaults` and the callback and health routes remain mapped. Do not change external package versions as part of this migration unless the current versions lack the required modern-hosting API; if that occurs, stop and report the blocker before changing dependencies.

## Out of scope

- Replacing Lamar, NLog, Sentry, or the existing middleware and endpoint implementations.
- Changing service configuration semantics, API routes, response formats, or health check behavior.
- Upgrading unrelated dependencies.
