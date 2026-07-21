# CSharpApp (.NET 9 Web API)

A production-ready, clean-architecture Web API built with **.NET 9**, **Minimal APIs**, and **MediatR (CQRS pattern)**. It includes resilient HTTP clients configured with **Polly**, custom delegating handlers, pipeline behaviors, and integration/unit tests using `WebApplicationFactory`.

---

## 🏗 Architecture & Key Concepts

### 1. CQRS with MediatR
All domain operations are separated into **Commands** and **Queries**:
* **Queries:** Fetch data via `IRequest<Result<T>>` (e.g., `GetProductsQuery`, `GetCategoriesQuery`).
* **Commands:** Mutate state via `IRequest<Result<T>>` (e.g., `CreateProductCommand`, `DeleteCategoryCommand`).
* **Pipelines:** Cross-cutting concerns are executed transparently via `IPipelineBehavior`:
  * `PerformanceBehaviour`: Traces execution times and logs slow requests.
  * `GlobalExceptionBehaviour`: Handles unhandled exceptions globally and standardizes failure responses.

### 2. Domain-Level `Result<T>` Mapping
Rather than throwing exceptions for flow control, handlers return an explicit `Result<T>` wrapper. Endpoints map these results cleanly to standard HTTP status codes:
* `Result.Success(data)` $\rightarrow$ `200 OK` / `201 Created`
* `Result.Fail("Not Found")` $\rightarrow$ `404 Not Found`
* `Result.Fail("Validation Error")` $\rightarrow$ `400 Bad Request`

### 3. Resilient HttpClient & Authentication Middleware
HTTP services (`ProductsService`, `CategoriesService`, `AuthService`) are configured using standard .NET Typed Clients and **Polly** policies for retry and wait strategies.

> ⚠️ **Important Architecture Note on `AuthTokenHandler`:**
> To avoid circular dependency loops (`System.Lazy` stack overflow during `CreateClient`), typed clients are split into two registration paths:
> * **Authenticated Services (`IProductsService`, `ICategoriesService`):** Configured with `AuthTokenHandler` to automatically inject Bearer tokens.
> * **Authentication Service (`IAuthService`):** Registered **without** `AuthTokenHandler` because it is responsible for fetching tokens in the first place.

---

## 📁 Solution Structure

```text
CSharpApp/
├── src/
│   ├── CSharpApp.Api/            # Minimal API Endpoints, OpenAPI/Swagger & Hosting
│   ├── CSharpApp.Application/    # CQRS Handlers, DTOs, Pipeline Behaviors & Services
│   ├── CSharpApp.Core/           # Core Entities, Interfaces & Result<T> Pattern
│   └── CSharpApp.Infrastructure/ # Typed HttpClients, Polly Policies & Auth Handlers
└── tests/
    └── CSharpApp.UnitTests/      # xUnit Tests (Handlers, Pipelines, WebApplicationFactory)
