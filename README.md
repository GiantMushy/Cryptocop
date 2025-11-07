# Cryptocop - Local Dev Stack

This project includes:

- .NET 9 Web API with JWT auth and OnTokenValidated blacklist check
- Payments and Emails workers (background services)
- Minimal React (Vite) test UI to register/sign in/sign out
- Docker Compose for one-command bring-up

## Quick start

Prerequisites: Docker Desktop 4.0+.

1. Build images

```
docker compose build
```

2. Start the stack

```
docker compose up -d
```

Services:

- API: http://localhost:5002
- Swagger: http://localhost:5002/swagger
- Web UI: http://localhost:5173
 - RabbitMQ Management: http://localhost:15672 (guest/guest)
 - Postgres: localhost:5432 (postgres/postgres)

3. Stop the stack

```
docker compose down
```

If you see a port conflict on 5002, change the host port in `docker-compose.yml`.

## Dev notes

- CORS is enabled in Development for `http://localhost:5173`.
- JWT settings live in `Cryptocop.Software.API/appsettings.json` under `Jwt`.
- React dev server reads `VITE_API_BASE_URL` from compose env.

### Database migrations

Migrations are applied in two ways:

- API attempts to apply pending EF Core migrations on startup in Development.
- You can force-apply migrations using a one-off .NET SDK container attached to the compose network:

```
docker run --rm \
	--network cryptocop_default \
	-v "$(pwd)":/src -w /src \
	mcr.microsoft.com/dotnet/sdk:9.0 bash -lc "\
		dotnet tool install -g dotnet-ef >/dev/null && \
		export PATH=\"$PATH:/root/.dotnet/tools\" && \
		dotnet ef database update \
			--project Cryptocop.Software.API.Repositories/Cryptocop.Software.API.Repositories.csproj \
			--startup-project Cryptocop.Software.API/Cryptocop.Software.API.csproj \
			--context CryptocopDbContext \
			--connection \"Host=postgres;Port=5432;Database=cryptocop;Username=postgres;Password=postgres\""
```

### JWT blacklist persistence

- JWTs include a `tokenId` claim. Sign-out blacklists the token in Postgres (`Tokens` table).
- Blacklisted tokens are rejected by an `OnTokenValidated` check across restarts.

