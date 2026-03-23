# AI CODER SYSTEM PROMPT (AGENT.md)

## 1. Project Overview
- **Name:** ReviewFilms API
- **Framework:** ASP.NET Core 10.0 LTS (Web API)
- **Language:** C# 14 (or latest supported)
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core (EF Core) - Code First
- **Architecture:** Layered Architecture (Monolithic Single Project)
- **Background:** The lead developer is transitioning from Spring Boot (Java). We aim for a structure that is familiar but strictly follows enterprise .NET best practices.

## 2. Folder Structure & Responsibility
AI must strictly follow this folder organization for a real-world Enterprise ASP.NET Core project:

- `/Controllers`: API Endpoints. Use `[ApiController]` and `[Route("api/[controller]")]`.
- `/Services`: Business logic implementation.
- `/Interfaces`: All Service interfaces (e.g., `IFilmService.cs`).
- `/Entities`: Database models (POCO classes). Equivalent to `@Entity` in JPA.
- `/DTOs`: Data Transfer Objects for Requests and Responses (organized by modules).
- `/Data`: `ApplicationDbContext`, EF Core Fluent API configurations, and data seeders.
- `/Migrations`: EF Core migration files.
- `/Middlewares`: Global exception handling and custom HTTP request pipelines.
- `/Security`: Handles everything related to system security (JWT generation, Password Hashing, Claims Transformation, Custom Authorization Policies/Handlers).
- `/Extensions`: `IServiceCollection` extension methods (e.g., `AddJwtAuth()`, `AddSwagger()`, `AddApplicationServices()`) to keep `Program.cs` clean and readable.
- `/Configurations`: Strongly-Typed classes used to map data from `appsettings.json` (e.g., `JwtSettings`, `CloudinarySettings`) via the `IOptions<T>` pattern.
- `/Enums`: Contains system constants and C# Enums mapping directly to PostgreSQL ENUM types (e.g., `UserStatus`, `MovieStatus`, `RoleCode`).

## 3. Coding Conventions (Strict)
- **Naming:** - Use `PascalCase` for folders, classes, methods, and public properties.
  - Use `camelCase` with an underscore prefix for private fields (e.g., `_filmService`).
- **Interfaces:** Must start with an `I` prefix (e.g., `IFilmService`, `IAuthService`).
- **Async/Await:** All I/O bound operations (Database, External APIs) must be asynchronous. Use `Task<T>` or `Task` as the return type. Append `Async` to method names (e.g., `GetFilmByIdAsync`).
- **Dependency Injection (DI):** Always use Constructor Injection. Register services using `AddScoped` for business logic via extension methods.
- **Data Access:** Use LINQ for querying. Prefer Method Syntax (e.g., `_context.Movies.Where(...)`) over Query Syntax.

## 4. Specific Patterns
- **DTO Mapping:** Never return Entities directly to the client. Always map Entities to Response DTOs. Ensure Request DTOs are validated.
- **Error Handling:** Avoid scattering `try-catch` blocks in Services/Controllers. Throw custom domain exceptions and catch them centrally using a Global Exception Handling Middleware.
- **EF Core:** Use the "Code First" approach. Migrations must be named descriptively (e.g., `AddMovieRatingTable`).

## 5. Technology Mapping (For AI Reference)
If the user asks for something "like Spring Boot", use this direct mapping:
- `@Service` -> Create Interface in `/Interfaces` + Class in `/Services` + Register in DI (`AddScoped`).
- `@RestController` -> `[ApiController]` in `/Controllers`.
- `JpaRepository` -> EF Core `DbSet<T>` in `ApplicationDbContext` (or a Repository pattern if explicitly requested).
- `application.yml` -> `appsettings.json`.
- `Flyway/Liquibase` -> EF Core Migrations.