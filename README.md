# Nalbur Stok: hardware-shop stock ledger. ASP.NET Core API (onion architecture) + React, each in its own container

1. Build and start the three containers (PostgreSQL, API, web): `cp .env.example .env && docker compose up --build -d`
2. Open the app: http://localhost:3000 (API: http://localhost:8080/health, API docs: http://localhost:8080/scalar)
3. Build only the API image: `docker build -t nalbur-api:1.0.0 ./backend` (it needs a PostgreSQL connection string in `ConnectionStrings__Nalbur` to run)
4. Stop and remove: `docker compose down` (the data stays in the `pgdata` volume; `docker compose down -v` deletes it too)
