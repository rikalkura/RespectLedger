# Respect Ledger

A gamified social application for tracking "Respect" transactions among friends. This application serves as a digital chronicle of friendship, combining Social Feed, RPG Mechanics, and Competitive Seasons.

## Project Overview

Respect Ledger is designed for a closed circle of friends to track social status, gratitude, and history through:

- **The Feed**: Records history and context (Why was the respect given?)
- **RPG System**: Long-term accumulation of status (Levels, Classes, Titles)
- **Seasons**: Short-term competition (Monthly leaderboards) to keep engagement high

## Technology Stack

### Backend
- **Framework**: .NET 10, ASP.NET Core Web API
- **Database**: Microsoft SQL Server (Azure SQL)
- **ORM**: Entity Framework Core
- **Architecture**: Clean Architecture with CQRS (MediatR)
- **Validation**: FluentValidation + Ardalis.GuardClauses
- **Authentication**: JWT Bearer Tokens

### Frontend
- **Framework**: React 19 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **Component Library**: shadcn/ui
- **State Management**: TanStack Query (React Query)
- **Routing**: React Router DOM v7
- **Animations**: Framer Motion

### Infrastructure
- **Database**: Azure SQL Database (Free Tier)
- **Backend Hosting**: Render.com (Free Tier) - Dockerized
- **Frontend Hosting**: Vercel
- **Image Storage**: Cloudinary (Free Tier)

## Architecture

The project follows **Strict Clean Architecture** with four layers:

1. **Domain Layer** (`RespectLedger.Domain`)
   - Pure entities, enums, value objects
   - No dependencies on other layers
   - Uses Result pattern for error handling

2. **Application Layer** (`RespectLedger.Application`)
   - CQRS handlers using MediatR
   - DTOs, validators (FluentValidation)
   - Interfaces for infrastructure services

3. **Infrastructure Layer** (`RespectLedger.Infrastructure`)
   - EF Core DbContext and repositories
   - External services (Cloudinary, Email)
   - JWT authentication implementation

4. **API Layer** (`RespectLedger.API`)
   - Thin controllers
   - Middleware (exception handling, CORS)
   - Dependency injection configuration

## Core Concepts

- **Respect**: The fundamental unit of currency. Acts as both a transaction of gratitude and Experience Points (XP).
- **Mana**: Daily allowance limiting how many respects a user can give per day (default: 3).
- **XP**: Lifetime counter of received respects. Determines User Level.
- **Season Score**: Monthly counter of received respects. Resets every month.
- **Class**: Dynamic title assigned based on the most frequent "Tags" in received respects.

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 18+ and npm/yarn/pnpm
- SQL Server (local or Azure SQL)
- Cloudinary account (for image storage)

### Backend Setup
```bash
# Restore NuGet packages
dotnet restore

# Update database connection string in appsettings.json
# Run migrations
dotnet ef database update --project RespectLedger.Infrastructure --startup-project RespectLedger.API

# Run the API
dotnet run --project RespectLedger.API
```

### Frontend Setup
```bash
cd RespectLedger.Client

# Install dependencies
npm install

# Run development server
npm run dev
```

## Project Structure

```
RespectLedger/
├── RespectLedger.Domain/          # Domain entities, enums, value objects
├── RespectLedger.Application/     # CQRS handlers, DTOs, validators
├── RespectLedger.Infrastructure/  # EF Core, repositories, external services
├── RespectLedger.API/             # Controllers, middleware, DI
├── RespectLedger.Client/          # React frontend application
└── docs/                          # Project documentation
```

## Development Guidelines

- Use file-scoped namespaces
- Use `record` for DTOs, Commands, and Queries
- Use `async/await` for all I/O operations
- Prefer explicit types over `var` when type is not obvious
- Commands return `Result` or `Result<T>`
- Queries use `.AsNoTracking()` for performance
- Commands fetch entities with tracking for modifications

## License

This is a private project for personal use among friends.
