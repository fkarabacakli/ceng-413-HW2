# Architecture

Nalbur Stok is a stock ledger for a small hardware shop (nalbur / hırdavat).
Every change to stock is a ledger line (in or out); the stock can never go below zero.

## Containers

```
Browser ──► web (nginx, :3000 → 8080) ──/api, /health──► api (ASP.NET Core, :8080) ──► db (PostgreSQL 17, :5432)
              serves the React build                      onion layers, EF Core           volume: pgdata
```

The browser only talks to `web`. nginx serves the static React files and forwards
`/api/*` and `/health` to the `api` container over the Compose network, so there is
one origin and no CORS in production. The API stores the ledger in the `db` container,
whose data lives in the named volume `pgdata`, so it survives `docker compose down`
(only `docker compose down -v` deletes it). Compose starts `db` first and the API only
after `pg_isready` succeeds.

## Backend layers (onion)

```
        ┌───────────────────────────────────────────┐
        │  Nalbur.Api  (endpoints, errors → HTTP, DI)│
        │  ┌─────────────────────────────────────┐  │
        │  │ Nalbur.Infrastructure                │  │
        │  │ (EF Core + PostgreSQL, seed)         │  │
        │  │  ┌───────────────────────────────┐   │  │
        │  │  │ Nalbur.Application            │   │  │
        │  │  │ (ProductService, DTOs,        │   │  │
        │  │  │  StockSettings)               │   │  │
        │  │  │  ┌─────────────────────────┐  │   │  │
        │  │  │  │ Nalbur.Domain           │  │   │  │
        │  │  │  │ Product, StockMovement, │  │   │  │
        │  │  │  │ rules, IProductRepository│ │   │  │
        │  │  │  └─────────────────────────┘  │   │  │
        │  │  └───────────────────────────────┘   │  │
        │  └─────────────────────────────────────┘  │
        └───────────────────────────────────────────┘
```

Every reference points **inward**. The repository interface lives in Domain and
Infrastructure implements it (dependency inversion). The API project is the
composition root and the only place that knows HTTP status codes.

| Project | References |
|---|---|
| Nalbur.Domain | – |
| Nalbur.Application | Domain |
| Nalbur.Infrastructure | Domain, EF Core, Npgsql |
| Nalbur.Api | Application, Infrastructure |
| Nalbur.Tests | Application (fake repository, fixed clock) |

## Persistence (code first)

The C# model is the source of truth. `NalburDbContext` and the Fluent API mappings in
`Nalbur.Infrastructure/Persistence/Configurations` describe the tables, so the Domain
classes stay free of EF attributes. EF Core reads and writes the private `_movements`
list and the private setters directly, so the stock rules still cannot be bypassed.

| Table | Holds |
|---|---|
| `products` | One row per product; `Sku` has a unique index |
| `stock_movements` | The ledger; `ProductId` → `products`, index on `CreatedAtUtc` |
| `__EFMigrationsHistory` | Which migrations have been applied |

The API applies pending migrations at startup (`Database.MigrateAsync`), so a fresh
database gets its tables on the first `docker compose up`. To change the schema, edit the
model or a configuration and add a migration:

```
cd backend
dotnet ef migrations add <Name> --project src/Nalbur.Infrastructure \
    --startup-project src/Nalbur.Api --output-dir Persistence/Migrations
```

## Business rules (Domain)

| Rule | Where | HTTP |
|---|---|---|
| Stock can never go below zero | `Product.Issue` | 409 |
| Stock code is unique (case-insensitive) | `ProductService.CreateAsync` | 409 |
| `Adet` and `Paket` accept whole numbers only | `Product` | 400 |
| Quantity must be greater than zero | `Product` | 400 |
| Stock code: letters, digits and `-`, max 32 | `Product.Create` | 400 |
| A product is critical when stock ≤ critical level | `Product.IsCritical` | – |
| Ledger lines are never edited; fix a mistake with an opposite movement | `StockMovement` | – |

## Endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/health` | Health check: 200 `healthy` when the API can reach PostgreSQL, 503 `unhealthy` otherwise |
| GET | `/api/info` | Shop name, default critical level, environment, version |
| GET | `/api/summary` | Product count, critical, out of stock, movements in the last 24 h |
| GET | `/api/products?search=&critical=true` | List (critical items first) |
| GET | `/api/products/{id}` | One product |
| POST | `/api/products` | `{ "sku", "name", "unit", "criticalLevel"?, "initialStock"? }` |
| PUT | `/api/products/{id}/critical-level` | `{ "criticalLevel": 6 }` |
| GET | `/api/products/{id}/movements` | Ledger of one product, newest first |
| POST | `/api/products/{id}/movements` | `{ "type": "In" \| "Out", "quantity": 3, "note"? }` |
| GET | `/api/movements?take=20` | Latest movements across all products |

`unit` is one of `Adet`, `Paket`, `Kg`, `Metre`, `Litre`.

## Environment variables

| Variable | Container | Default | Purpose |
|---|---|---|---|
| `SHOP_NAME` | api | `Nalbur Stok` | Shown in the header and `/api/info` |
| `CRITICAL_STOCK_DEFAULT` | api | `5` | Critical level for new products that do not give one |
| `SEED_SAMPLE_DATA` | api | `false` (`true` in Compose) | Load nine sample products with two weeks of history |
| `ConnectionStrings__Nalbur` | api | none (required) | PostgreSQL connection string; Compose builds it from `POSTGRES_PASSWORD` |
| `ASPNETCORE_HTTP_PORTS` | api | `8080` | Port Kestrel listens on |
| `ALLOWED_ORIGINS` | api | empty | CORS origins, only for local `npm run dev` |
| `API_UPSTREAM` | web | `http://api:8080` | Where nginx forwards `/api` |
| `POSTGRES_PASSWORD` | db, api | none (required) | Database password, set in `.env`; Compose stops with an error if it is missing |
| `POSTGRES_PORT` | db | `5432` | Host port for local development and DB tools |

## Try it with curl

```
curl localhost:8080/api/summary
curl -X POST localhost:8080/api/products -H 'Content-Type: application/json' \
     -d '{"sku":"ANK-10","name":"Ankraj 10 mm","unit":"Adet","initialStock":20}'
# then sell more than there is → 409 "Stok yetersiz"
curl -X POST localhost:8080/api/products/<id>/movements -H 'Content-Type: application/json' \
     -d '{"type":"Out","quantity":25}'
```

## Local development without Docker

```
docker compose up -d db                                # PostgreSQL on localhost:5432
cd backend
# once: keep the connection string in user secrets (outside the repository)
dotnet user-secrets set ConnectionStrings:Nalbur \
  "Host=localhost;Port=5432;Database=nalbur;Username=nalbur;Password=<POSTGRES_PASSWORD from .env>" \
  --project src/Nalbur.Api
dotnet run --project src/Nalbur.Api                    # http://localhost:5080
cd frontend && npm install && npm run dev              # http://localhost:5173
```
