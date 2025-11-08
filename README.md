# Cryptocop

Feature showcase for a small crypto shop backend + minimal UI.

## Stack & Architecture
* ASP.NET Core 9 Web API
* JWT auth with token blacklist (persistent sign-out)
* RabbitMQ + two workers (Payments, Emails)
* PostgreSQL via EF Core (auto migrations on startup)
* React (Vite) web client (register, sign in, cart interaction)
* Docker Compose orchestration

## Domain Features
* Account: register, sign in/out, JWT issuance & revocation
* Addresses: CRUD per user
* Shopping cart: fractional quantities (0.01), add/update/delete, price lookup
* Payments & Orders: publish to queue; workers consume (mock processing + email)
* Cryptocurrencies: asset listing & price endpoint (priceUsd + priceInUsd)
* Markets ("exchanges" endpoint): sourced from instructor mock service

## Technical Highlights
* Centralized ProblemDetails error responses
* Token blacklist enforced in `OnTokenValidated` for immediate revocation
* Rounding & validation to avoid float precision issues (cart quantities/prices)
* HttpClient-based external service layer (crypto + markets) with fallback logic
* Postman collection included for automated endpoint tests (`Postman/Cryptocop.postman_collection.json`)

## Postman (Tests)
Import collection, run Register then Sign in, execute remaining requests. CLI: `newman run Postman/Cryptocop.postman_collection.json --reporters cli`.

## Directories
* `Cryptocop.Software.API` – controllers, middleware, DI setup
* `Cryptocop.Software.API.Models` – DTOs, envelopes, input models
* `Cryptocop.Software.API.Repositories` – EF Core entities & data access
* `Cryptocop.Software.API.Services` – business logic & external API calls
* `Cryptocop.Software.Worker.Payments` / `...Emails` – background queue consumers
* `web/` – Vite React client
* `Postman/` – test collection

## Notes
* Markets & asset data use the provided mock base URL via compose env.
* Email worker expects a `SENDGRID_API_KEY` (empty by default).
* All auth-required endpoints protected by fallback authorization; public ones marked `[AllowAnonymous]`.

---
This README intentionally concise: it enumerates capabilities rather than full run instructions.

