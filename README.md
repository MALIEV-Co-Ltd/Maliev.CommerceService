# MALIEV Commerce Service

Storefront commerce service for MALIEV public web properties.

## Architecture

`Maliev.CommerceService` owns e-commerce catalog, carts, checkout sessions, and shop orders for MALIEV products, SimMount products, spare parts, pneumatic injection molding machines, and 3D printed products. It does not own manufacturing project quotes or custom manufacturing orders; those remain in `Maliev.QuoteEngine`, `Maliev.ProjectService`, and manufacturing order services.

## Starter Catalog Data

On startup the service seeds draft starter listings for the MALIEV pneumatic injection molding machine 30g and 50g lineup. These records are intentionally draft data for employees to refine in `Maliev.Intranet`; they are not treated as a final Shopify import and are not shown by anonymous product listing APIs until published.

## Customer Boundary

All authenticated commerce records reference the canonical `CustomerId` from `Maliev.CustomerService`. Commerce does not create or store duplicate customer profiles.

## API Endpoints

| Method | Route | Purpose | Auth |
| --- | --- | --- | --- |
| GET | `/commerce/v1/products` | List published products | Anonymous |
| GET | `/commerce/v1/products/{handle}` | Get product detail | Anonymous |
| POST | `/commerce/v1/products` | Create product | `commerce.products.create` |
| PATCH | `/commerce/v1/products/{id}` | Update product | `commerce.products.update` |
| GET | `/commerce/v1/collections` | List collections | Anonymous |
| POST | `/commerce/v1/collections` | Create collection | `commerce.collections.create` |
| POST | `/commerce/v1/carts` | Create cart | `commerce.carts.create` |
| GET | `/commerce/v1/carts/{id}` | Get cart | `commerce.carts.read` |
| POST | `/commerce/v1/carts/{id}/lines` | Add/update cart line | `commerce.carts.update` |
| DELETE | `/commerce/v1/carts/{id}/lines/{lineId}` | Remove cart line | `commerce.carts.update` |
| POST | `/commerce/v1/checkout-sessions` | Create checkout session | `commerce.checkouts.create` |
| POST | `/commerce/v1/checkout-sessions/{id}/complete` | Convert checkout session to shop order | `commerce.orders.create` |
| GET | `/commerce/v1/orders` | List shop orders | `commerce.orders.read` |
| GET | `/commerce/v1/orders/{id}` | Get shop order | `commerce.orders.read` |

## Permissions

Permissions use the `commerce.{plural-resource}.{action}` format and are defined in `CommercePermissions`.

## Local Verification

```powershell
dotnet build Maliev.CommerceService.slnx --configuration Release
dotnet test Maliev.CommerceService.slnx --configuration Release
```
