# Maliev.CommerceService Agent Guidelines

This repository owns MALIEV storefront commerce: product catalog, collections, carts, checkout sessions, and shop orders.

## Build, Test, And Format

Run commands from `B:\maliev\Maliev.CommerceService`.

```powershell
dotnet build Maliev.CommerceService.slnx --configuration Release
dotnet test Maliev.CommerceService.slnx --configuration Release
dotnet format Maliev.CommerceService.slnx --verify-no-changes
```

Use focused tests while iterating:

```powershell
dotnet test Maliev.CommerceService.slnx --configuration Release --filter "FullyQualifiedName~CommerceServiceTests"
```

## Repository Structure

- `Maliev.CommerceService.Api`: controllers, API startup, endpoint authorization.
- `Maliev.CommerceService.Application`: DTOs, service interfaces, use-case orchestration, permission constants.
- `Maliev.CommerceService.Domain`: commerce entities and domain rules.
- `Maliev.CommerceService.Infrastructure`: EF Core persistence, repositories, startup seeding.
- `Maliev.CommerceService.Tests`: xUnit unit and integration tests.

## Service Boundaries

- Commerce owns standard product storefront data, carts, checkout sessions, and shop orders.
- Commerce does not own custom manufacturing quotes, DFM, project pricing, or formal quote PDFs. Keep those in `Maliev.QuoteEngine`, `Maliev.ProjectService`, `Maliev.QuotationService`, `Maliev.PdfService`, and manufacturing order services.
- Authenticated commerce records must reference canonical `CustomerId` values from `Maliev.CustomerService`; do not create duplicate customer profile state here.
- Starter machine listings are draft seed data only. Anonymous catalog APIs must expose published catalog data, not employee draft records.

## Authorization And API Rules

- All write, cart, checkout, management, and order endpoints require `[RequirePermission]`.
- Anonymous access is only for published product and published collection reads.
- Permissions use `commerce.{plural-resource}.{action}` and must be defined centrally in `CommercePermissions`.
- Before changing controllers, DTOs, BFF clients, or downstream calls, verify the wire contract end to end: request DTOs, response DTOs, JSON names, route names, and tests.
- Never trust browser-supplied customer IDs. Resolve or verify customer scope server-side before exposing carts, checkout sessions, or shop orders.

## Testing Rules

- Use xUnit with standard `Assert.*`; do not use FluentAssertions.
- Use PostgreSQL/Testcontainers for integration behavior. Do not use EF InMemory.
- Add authorization regression tests for new or changed endpoints, including anonymous denial for non-public endpoints.
- Add contract/source tests when changing API routes, JSON shape, permission metadata, or seed visibility.

## Banned Libraries And Practices

- AutoMapper is banned; use explicit mapping.
- FluentValidation is banned; use DataAnnotations or manual validation.
- Swashbuckle/Swagger is banned; use Scalar if API docs are added.
- Do not commit secrets or test credentials. Use environment variables or GCP Secret Manager through Aspire/GitOps.

## Git Rules

- This is an independent git repo. Run git commands from `B:\maliev\Maliev.CommerceService`.
- Commit every meaningful repo-local change after validation.
- Do not push unless the user explicitly asks.
