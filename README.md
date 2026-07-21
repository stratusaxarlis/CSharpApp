# CSharpApp (.NET 9 Web API)

A production-ready, clean-architecture Web API built with **.NET 9**, **Minimal APIs**, **Scalar API Documentation**, and **MediatR (CQRS pattern)**. It includes resilient HTTP clients configured with **Polly**, custom delegating handlers, pipeline behaviors, and integration/unit tests using `WebApplicationFactory`.

---

## 🏗 Architecture & Key Concepts

### 1. CQRS with MediatR
All domain operations are separated into **Commands** and **Queries**:
* **Queries:** Fetch data via `IRequest<Result<T>>` (e.g., `GetProductsQuery`, `GetCategoriesQuery`).
* **Commands:** Mutate state via `IRequest<Result<T>>` (e.g., `CreateProductCommand`, `DeleteCategoryCommand`).
* **Pipelines:** Cross-cutting concerns are executed transparently via `IPipelineBehavior`:
  * `PerformanceBehaviour`: Traces execution times and logs slow requests.
  * `GlobalExceptionBehaviour`: Handles unhandled exceptions globally and standardizes failure responses.

### 2. Native OpenAPI & Scalar API Documentation
With .NET 9 deprecating built-in Swashbuckle, this project leverages native document generation (`Microsoft.AspNetCore.OpenApi`) paired with **Scalar** (`Scalar.AspNetCore`) for modern, high-performance, and interactive API documentation:
* **Native Spec Generation:** Generates OpenAPI v3 specification at `/openapi/v1.json`.
* **Scalar UI:** Mounted at `/scalar/v1` with dark mode support, built-in request testing, and automatic code-snippet generation across multiple languages.

### 3. Domain-Level `Result<T>` Mapping
Rather than throwing exceptions for flow control, handlers return an explicit `Result<T>` wrapper. Endpoints map these results cleanly to standard HTTP status codes:
* `Result.Success(data)` $\rightarrow$ `200 OK` / `201 Created`
* `Result.Fail("Not Found")` $\rightarrow$ `404 Not Found`
* `Result.Fail("Validation Error")` $\rightarrow$ `400 Bad Request`

### 4. Resilient HttpClient & Authentication Middleware
HTTP services (`ProductsService`, `CategoriesService`, `AuthService`) are configured using standard .NET Typed Clients and **Polly** policies for retry and wait strategies.

> ⚠️ **Important Architecture Note on `AuthTokenHandler`:**
> To avoid circular dependency loops (`System.Lazy` stack overflow during `CreateClient`), typed clients are split into two registration paths:
> * **Authenticated Services (`IProductsService`, `ICategoriesService`):** Configured with `AuthTokenHandler` to automatically inject Bearer tokens.
> * **Authentication Service (`IAuthService`):** Registered **without** `AuthTokenHandler` because it is responsible for fetching tokens in the first place.

# CSharpApp (.NET 9 Web API)

A production-ready, clean-architecture Web API built with **.NET 9**, **Minimal APIs**, **Scalar API Documentation**, and **MediatR (CQRS pattern)**. It includes resilient HTTP clients configured with **Polly**, custom delegating handlers, pipeline behaviors, and integration/unit tests using `WebApplicationFactory`.

---

## 🚀 Getting Started

Follow these instructions to clone, build, run, and test the API on your local machine.

### Prerequisites

Ensure you have the following installed before starting:
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) *(Optional — required only for container deployment)*
* Git CLI

---

### Step 1: Clone the Repository

```bash
git clone [https://github.com/stratusaxarlis/CSharpApp.git](https://github.com/stratusaxarlis/CSharpApp.git)
cd CSharpApp

---

## 📁 Solution Structure

```text
CSharpApp/
├── src/
│   ├── CSharpApp.Api/            # Minimal API Endpoints, Native OpenAPI/Scalar & Hosting
│   ├── CSharpApp.Application/    # CQRS Handlers, DTOs, Pipeline Behaviors & Services
│   ├── CSharpApp.Core/           # Core Entities, Interfaces & Result<T> Pattern
│   └── CSharpApp.Infrastructure/ # Typed HttpClients, Polly Policies & Auth Handlers
└── tests/
    └── CSharpApp.UnitTests/      # xUnit Tests (Handlers, Pipelines, WebApplicationFactory)
