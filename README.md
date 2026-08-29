# RideSharing Platform

A full-stack ride-sharing platform with a .NET backend API and an AI-powered chatbot.

## Architecture

- **Frontend** (React/Angular, port 3000/4200) communicates with the **AI Chatbot** (Python/FastAPI, port 8000) and the **.NET Backend API** (port 5000)
- **.NET Backend API**: ASP.NET Core, MediatR, EF Core, SignalR, JWT Auth, Stripe, RabbitMQ, Redis Cache
- **Clean Architecture**: API -> Application -> Infrastructure -> Core
- **Data stores**: SQL Server 2022 (port 1433), Redis (port 6379)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | C# (.NET 10), ASP.NET Core |
| Database | SQL Server 2022, Entity Framework Core |
| Caching | Redis (StackExchange.Redis) |
| Messaging | RabbitMQ |
| Payments | Stripe |
| Auth | JWT Bearer + Refresh Tokens |
| Real-time | SignalR |
| AI Chatbot | Python, FastAPI, LangGraph, Google Gemini |
| Testing | xUnit, Moq, FluentAssertions, Testcontainers |

## Quick Start

### Prerequisites
- Docker Desktop
- .NET 10 SDK (for local development)

### Run with Docker

```bash
cp .env.example .env
# Edit .env with your keys (Stripe, JWT, Google API)
docker-compose up --build
```

Services:
- API: http://localhost:5000 (Swagger: http://localhost:5000/swagger)
- Chatbot: http://localhost:8000
- RabbitMQ Mgmt: http://localhost:15672 (default credentials configurable via environment variables)

### Run Locally

```bash
cd src/RideSharing.Api
dotnet run
```

## API Endpoints

### Authentication
| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/users/register` | Register new user |
| POST | `/api/users/login` | Login, returns JWT + refresh token |
| POST | `/api/users/refresh` | Refresh expired JWT |

### Rides
| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/rides` | Request a ride |
| GET | `/api/rides/{id}` | Get ride details |
| POST | `/api/rides/{id}/accept` | Driver accepts ride |
| POST | `/api/rides/{id}/start` | Start ride |
| POST | `/api/rides/{id}/complete` | Complete ride |
| POST | `/api/rides/{id}/cancel` | Cancel ride |
| GET | `/api/rides/active` | Get active rides |

### Drivers
| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/drivers/nearby` | Find nearby drivers |
| PUT | `/api/drivers/{id}/location` | Update location |
| PUT | `/api/drivers/{id}/availability` | Go online/offline |
| PUT | `/api/drivers/{id}/verify` | Admin: verify driver |

### Chatbot
| Method | Path | Description |
|--------|------|-------------|
| POST | `/chat` | Send message to AI chatbot |
| GET | `/health` | Health check |

## Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `SA_PASSWORD` | Yes | — | SQL Server SA password |
| `STRIPE_SECRET_KEY` | Yes | — | Stripe API secret key |
| `JWT_KEY` | Yes | — | JWT signing key (min 32 chars) |
| `GOOGLE_API_KEY` | Yes | — | Google Gemini API key |
| `GOOGLE_MAPS_API_KEY` | No | — | Google Maps API key |
| `RABBITMQ_USER` | No | `guest` | RabbitMQ username |
| `RABBITMQ_PASS` | No | `guest` | RabbitMQ password |

## Testing

```bash
# .NET tests
dotnet test RideSharing.sln

# Chatbot tests
cd chatbot && python -m pytest . -v
```

## Project Structure

```
src/
├── RideSharing.Core/          # Domain entities, value objects, enums
├── RideSharing.Application/   # CQRS use cases, DTOs, interfaces, validators
├── RideSharing.Infrastructure/ # EF Core, Redis, Stripe, RabbitMQ, maps
└── RideSharing.Api/           # Controllers, middleware, SignalR hub

chatbot/                       # AI chatbot (Python/FastAPI/LangGraph)

tests/
├── RideSharing.UnitTests/     # xUnit unit tests with Moq
└── RideSharing.IntegrationTests/ # Integration tests with Testcontainers
```
