# FlightBoard

A full‑stack, real‑time flight board application built with a clean architecture and test‑driven development.

## Tech Stack
- **Backend:** ASP.NET Core 8 (C#), Entity Framework Core, SQLite (persistent DB)
- **Real‑Time:** SignalR (hub at `/hubs/flights`)
- **Frontend:** React + TypeScript, Vite, Material UI
- **State:** Redux Toolkit (UI state) + TanStack Query (server state)
- **Tests:** xUnit + Moq, FluentAssertions
- **Containers:** Dockerfiles for API and Web, `docker-compose.yml` with Nginx reverse‑proxy

## Project Structure
```
FlightBoard.sln
backend/
  src/FlightBoard.API/
  src/FlightBoard.Application/
  src/FlightBoard.Domain/
  src/FlightBoard.Infrastructure/
  src/FlightBoard.Common/
  tests/FlightBoard.UnitTests/
frontend/
docker-compose.yml
```
- **Domain** – entities (`Flight`, `FlightStatus`)
- **Application** – DTOs, validation (FluentValidation), `IFlightStatusCalculator`
- **Infrastructure** – EF Core `FlightDbContext` (SQLite), unique index on `FlightNumber`, seeding
- **API** – Controllers, Swagger (enabled in Development), SignalR Hub

## Business Rules (server‑calculated status)
The flight **status is computed on the server** (single source of truth) based on `DateTime.UtcNow`:
- **Scheduled:** more than 30 minutes before departure
- **Boarding:** from 30 minutes before until the scheduled departure time
- **Departed:** from the scheduled time up to +60 minutes
- **Landed:** more than 60 minutes after the scheduled time

## Validation (server‑side)
- Required: `FlightNumber` (must be unique), `Destination`, `Gate`, `ScheduledTime`
- `ScheduledTime` must be in the future
- Returns proper HTTP codes: `400 Bad Request` for validation errors, `409 Conflict` for duplicate `FlightNumber`

## Run Locally
Backend:
```cmd
dotnet build FlightBoard.sln
set DOTNET_ROLL_FORWARD=Major
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project backend\src\FlightBoard.API\FlightBoard.API.csproj --urls http://localhost:5000
```
Frontend:
```cmd
cd frontend
set VITE_API_URL=http://localhost:5000
npm i
npm run dev
```
- UI: http://localhost:5173
- Swagger: http://localhost:5000/swagger

## Docker Compose (production‑like)
```bash
docker compose up --build
```
- Web (Nginx): http://localhost:8081
- API: http://localhost:8080  (Swagger is **enabled** because the compose sets `ASPNETCORE_ENVIRONMENT=Development`)

## API Quickstart
- `GET /api/flights` — list flights (status is computed on the server)
- `GET /api/flights/search?status=&destination=`
- `GET /api/flights/{id}` — get a single flight
- `POST /api/flights` — create a flight
- `PUT /api/flights/{id}` — update a flight
- `DELETE /api/flights/{id}` — delete a flight
- `GET /health` — health probe
- `GET /swagger` — Swagger UI (Development environment)

## Tests
```cmd
dotnet test backend\tests\FlightBoard.UnitTests\FlightBoard.UnitTests.csproj
```
Includes calculator tests; extend with validation/controller tests as needed.

## Notes
- In Docker, the frontend uses **relative** `/api` so no CORS is required; Nginx proxies `/api` and `/hubs` to the API.
- `frontend/src/vite-env.d.ts` provides Vite/TS types for `import.meta.env` to ensure clean builds.