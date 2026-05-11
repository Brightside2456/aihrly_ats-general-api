# Aihrly ATS General API

A tiny slice of an Applicant Tracking System (ATS) built with ASP.NET Core 9.0 and PostgreSQL.

## Features
- **Jobs API:** Create, list (with filter/pagination), and get jobs.
- **Applications API:** 
    - Candidate application submission.
    - Pipeline management with stage transition validation.
    - Application notes with different types.
    - Multi-dimensional scoring (Culture Fit, Interview, Assessment).
- **Background Notifications:** Asynchronous notifications when an application is `Hired` or `Rejected` (Part 2 - Option A).
- **Audit Trail:** Automatic recording of every stage change.
- **Header-based Auth:** All mutating/internal requests require `X-Team-Member-Id`.

## Tech Stack
- **Framework:** .NET 9.0 (ASP.NET Core Web API)
- **Database:** PostgreSQL (via Entity Framework Core & Npgsql)
- **Testing:** xUnit, Microsoft.AspNetCore.Mvc.Testing, InMemoryDatabase
- **Docs:** Swagger/OpenAPI

## How to Run

### 1. Prerequisites
- .NET 9.0 SDK
- Docker (optional, for Postgres)

### 2. Run with Docker (Recommended for DB)
A `docker-compose.yml` is provided to spin up the PostgreSQL database.
```bash
docker compose up -d
```

### 3. Run the API
```bash
cd AihrlyATSGeneralAPI
dotnet run
```
The API will be available at `https://localhost:5001` (or see console output). Swagger UI is available at `/swagger`.

## How to Run Tests
```bash
dotnet test
```
The solution contains 13 tests covering unit logic and full integration flows.

## Seeding & Team Members
The database is automatically seeded with 2 team members on startup (if not in Testing environment):
- **ID 1:** Alice Recruiter
- **ID 2:** Bob Manager

Use these IDs in the `X-Team-Member-Id` header for protected endpoints.

## Design Decisions & Trade-offs
- **Background Jobs:** Used `System.Threading.Channels` and `BackgroundService` for a lightweight, built-in asynchronous notification system. For a production app, Hangfire would be preferred for persistence and retries.
- **Separation of Concerns:** DTOs are used for all API inputs/outputs to prevent leaking database models and to allow independent evolution of the API and schema.
- **Validation:** Implemented a `PipelineService` to encapsulate stage transition logic, keeping the controller thin and making the business rules easily testable.
- **Authentication:** A custom `AuthorizeTeamMember` filter handles the simple header-based identification required for the assessment.

## Assumptions
- "A candidate cannot apply to the same job twice with the same email" is enforced via a unique index on (JobId, CandidateEmail).
- For Part 2, "dispatched asynchronously" means the API returns 204 immediately, and the work happens on a background thread.
- "Terminal" stages (Hired/Rejected) do not allow further transitions.

## Future Improvements
- Implement Hangfire for reliable background job processing.
- Add more exhaustive validation (e.g., using FluentValidation).
- Implement real authentication (OIDC/JWT) if this were a production system.
- Add more granular logging and metrics.
