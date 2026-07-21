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


# Architectural Suggestions & Future Roadmap

This document outlines strategic enhancements for the **CSharpApp** repository to drastically improve performance, resilience, and scalability. The roadmap is divided into two major phases: implementing an advanced hybrid caching layer and evolving the architecture into a Modular Monolith for future microservices readiness.

---

## 🚀 Phase 1: Advanced Hybrid Caching & Resilience

To reduce database load and improve response times, we should implement a robust caching strategy across the API and external service layers.

### 1. Hybrid Cache Implementation (FusionCache + Redis)
Standard in-memory caching is lost on app restarts, and pure Redis caching introduces network latency. The solution is **Hybrid Caching** using [ZiggyCreatures.FusionCache](https://github.com/ZiggyCreatures/FusionCache).
* **L1 Cache (Memory):** Fast, local RAM caching for instant retrieval.
* **L2 Cache (Distributed):** Redis as the distributed secondary layer.
* **Redis Backplane:** Ensures that if one API instance updates the cache, all other API instances immediately invalidate their L1 cache, preventing stale data.

### 2. Endpoint Response Caching & Invalidation
We will apply caching to our high-traffic `GET` endpoints, tied directly to our MediatR CQRS pipeline.

* **Caching Queries:** `GetProductsQuery` and `GetCategoriesQuery` will check FusionCache first. If the data is missing, it hits the database and populates both L1 and L2 caches.
* **Cache Invalidation on CRUD:** We will implement an `ICacheInvalidator` or a MediatR `IPipelineBehavior`. 
  * When a `CreateProductCommand`, `UpdateProductCommand`, or `DeleteProductCommand` is executed successfully, the pipeline will immediately invalidate related cache keys (e.g., `cache:products:all` and `cache:products:{id}`).
  * This guarantees that subsequent `GET` requests fetch fresh data while maintaining high read throughput.

### 3. Auth Token Caching & Automatic Refresh Mechanism
Currently, fetching a new auth token for every outgoing request is inefficient. We will optimize this using FusionCache and our existing `DelegatingHandler`.

* **Token Caching:** The JWT/Bearer token will be cached in FusionCache with an expiration slightly shorter than the token's actual Time-To-Live (TTL).
* **Graceful Refresh on 401/403:** 
  We will update our custom `AuthTokenHandler` (the `DelegatingHandler` wrapping our HttpClients). 
  1. The handler intercepts an outgoing request, grabs the token from FusionCache, and attaches it.
  2. If the external API responds with `401 Unauthorized` or `403 Forbidden` (indicating the token expired or was revoked), the handler traps the response.
  3. The handler triggers a background call to the Auth service to get a **new** token, updates FusionCache with the new token, and **retries the original request** seamlessly.
  4. The client consuming our API never knows the token expired; the request succeeds automatically.

---

## 🏗 Phase 2: Evolving to a Modular Monolith

While the current Clean Architecture separates concerns by technical layers (Core, Application, Infrastructure), as the application grows, domain concepts (Products, Categories, Orders) become tightly coupled across those layers.

To prepare the application for massive scale and a potential transition to Microservices, we should adopt a **Modular Monolith Architecture**.

### The Template: `ModularMonolithExample`
We will use [https://github.com/stratusaxarlis/ModularMonolithExample](https://github.com/stratusaxarlis/ModularMonolithExample) as our structural blueprint.

### Why a Modular Monolith?
Instead of slicing the app by *technical layers* (UI $\rightarrow$ BLL $\rightarrow$ DAL), we slice it by *business capabilities* (Modules).

1. **Strict Isolation:** The `Products` module and the `Categories` module will have their own independent Application, Core, and Infrastructure layers.
2. **No Direct Database Joins:** Modules cannot directly query each other's database tables. If the `Products` module needs category data, it communicates via an in-memory event bus (MediatR) or explicit module-to-module public interfaces.
3. **Independent Data Storage:** Each module can theoretically use a different database schema (or entirely different database technologies, e.g., SQL for Products, Redis for Caching).

### The Path to Microservices
A Modular Monolith gives us the exact boundaries of microservices without the operational overhead of Kubernetes, distributed tracing, and network latency right out of the gate. 

If the `Products` feature ever experiences massive traffic that `Categories` does not, we simply lift the `Products` module out of the solution, wrap it in its own Web API project, and deploy it as an independent Microservice. Because the boundaries and module communications were already strictly enforced, this extraction takes hours instead of months.

---

## 📋 Summary of Next Steps

1. **Install & Configure FusionCache** with a Redis provider and Redis backplane.
2. **Implement MediatR Caching Behaviors** to handle automatic fetching and invalidation for CQRS pipelines.
3. **Refactor `AuthTokenHandler`** to cache the JWT and implement the `401/403` intercept-and-refresh retry policy.
4. **Begin architectural restructuring**, gradually moving domain logic from a monolithic Clean Architecture into isolated feature modules mirroring `ModularMonolithExample`.

└── tests/
    └── CSharpApp.UnitTests/      # xUnit Tests (Handlers, Pipelines, WebApplicationFactory)
